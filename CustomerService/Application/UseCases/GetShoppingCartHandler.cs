 using Application.DTO.Request;
using Application.DTO.Response;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IDatabase = StackExchange.Redis.IDatabase;

namespace Application.UseCases
{
    public class GetShoppingCartHandler
    {
        private readonly ICartRepository _cartRepository;
        private readonly IInventoryServiceClient _inventoryServiceClient;
        private readonly IMapper _mapper;
        private readonly IRedisCacheService _redisCacheService;

        public GetShoppingCartHandler(
            ICartRepository cartRepository,
            IInventoryServiceClient inventoryServiceClient,
            IMapper mapper,
            IRedisCacheService redisCacheService)
        {
            _cartRepository = cartRepository;
            _inventoryServiceClient = inventoryServiceClient;
            _mapper = mapper;
            _redisCacheService = redisCacheService;
        }

        private string GetCartKey(int accountId) => $"cart:{accountId}";

        public async Task<ResponseDTO<List<CartItemResponse>>> Handle(int accountId)
        {
            var cartKey = GetCartKey(accountId);
            // 1. Kiểm tra giỏ hàng trong Redis
            var cart = await _redisCacheService.GetCacheAsync<List<CartItem>>(cartKey) ?? new List<CartItem>();

            // 2. Nếu cache trống, tải từ DB và cập nhật cache
            if (!cart.Any())
            {
                Console.WriteLine("⏳ Cache miss: Lấy giỏ hàng từ Database...");
                cart = await _cartRepository.GetCartFromDatabaseAsync(accountId);
                if (cart.Any())
                {
                    await _redisCacheService.SetCacheAsync(cartKey, cart, TimeSpan.FromMinutes(30));
                }
            }

            // 3. Nếu vẫn trống, trả về giỏ hàng rỗng
            if (!cart.Any())
            {
                return new ResponseDTO<List<CartItemResponse>>(new List<CartItemResponse>(), true, "Giỏ hàng trống.");
            }

            // 4. Ánh xạ sang response
            var cartItemResponses = _mapper.Map<List<CartItemResponse>>(cart);

            // 5. Lấy thông tin sản phẩm song song từ InventoryService
            var tasks = cartItemResponses.Select(async item =>
            {
                var productVariant = await _inventoryServiceClient.GetProductVariantById(item.ProductVariantId);
                if (productVariant != null)
                {
                    item.ProductName = productVariant.ProductName;
                    item.ImagePath = productVariant.ImagePath;
                    item.Size = productVariant.Size;
                    item.Color = productVariant.Color;
                    item.Price = productVariant.Price;
                    item.DiscountedPrice = productVariant.DiscountedPrice;
                    item.PromotionTitle = productVariant.PromotionTitle;
                }
            });
            await Task.WhenAll(tasks);

            return new ResponseDTO<List<CartItemResponse>>(cartItemResponses, true, "Lấy giỏ hàng thành công!");
        }

        public async Task<ResponseDTO<bool>> AddCartItem(int accountId, AddToCartRequest cartItemDto)
        {
            // 🔍 1. Lấy ProductVariant từ InventoryService dựa trên ProductId, Size, Color
            var productVariant = await _inventoryServiceClient.GetProductVariantByDetails(cartItemDto.ProductId, cartItemDto.Size, cartItemDto.Color);

            if (productVariant == null)
            {
                return new ResponseDTO<bool>(false, false, "Sản phẩm với kích thước và màu sắc không tồn tại!");
            }

            if (productVariant.StockQuantity < cartItemDto.Quantity)
            {
                return new ResponseDTO<bool>(false, false, "Số lượng sản phẩm không đủ!");
            }

            var cartKey = GetCartKey(accountId);

            // 🛒 2. Lấy giỏ hàng từ Redis; nếu không có thì tải từ DB
            var cart = await _redisCacheService.GetCacheAsync<List<CartItem>>(cartKey);
            if (cart == null)
            {
                cart = await _cartRepository.GetCartFromDatabaseAsync(accountId) ?? new List<CartItem>();
            }

            // 🔄 3. Cập nhật giỏ hàng trong bộ nhớ
            var existingItem = cart.FirstOrDefault(c => c.ProductVariantId == productVariant.VariantId);
            if (existingItem != null)
            {
                existingItem.Quantity += cartItemDto.Quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductVariantId = productVariant.VariantId,
                    Quantity = cartItemDto.Quantity
                });
            }

            // 💾 4. Cập nhật dữ liệu vào DB qua repository
            await _cartRepository.AddToCartAsync(accountId, new CartItem
            {
                ProductVariantId = productVariant.VariantId,
                Quantity = cartItemDto.Quantity
            });

            // 🔄 5. Cập nhật cache Redis với giỏ hàng mới nhất
            await _redisCacheService.SetCacheAsync(cartKey, cart, TimeSpan.FromMinutes(30));

            return new ResponseDTO<bool>(true, true, "Thêm sản phẩm vào giỏ hàng thành công!");
        }

        // --- Remove Cart Item ---
        public async Task RemoveCartItem(int accountId, int productVariantId)
        {
            // Cập nhật DB
            await _cartRepository.RemoveFromCartAsync(accountId, productVariantId);
            // Xóa cache hoặc cập nhật lại cache nếu cần
            await _redisCacheService.RemoveCacheAsync(GetCartKey(accountId));
        }

        // --- Clear Cart ---
        public async Task ClearCart(int accountId)
        {
            // Xóa giỏ hàng trong DB
            await _cartRepository.ClearCartInDatabase(accountId);
            // Xóa cache
            await _redisCacheService.RemoveCacheAsync(GetCartKey(accountId));
        }

        // --- Sync Cart to Database ---
        public async Task SyncCartToDatabase(int accountId)
        {
            // Lấy giỏ hàng từ Redis
            var cart = await _redisCacheService.GetCacheAsync<List<CartItem>>(GetCartKey(accountId));
            if (cart == null || !cart.Any()) return;

            await _cartRepository.SyncCartToDatabase(accountId, cart);
        }

        // --- Clear Cart After Order ---
        public async Task ClearCartAfterOrderAsync(int accountId, List<int> selectedProductVariantIds)
        {
            // ✅ Xóa sản phẩm đã đặt hàng khỏi DB
            await _cartRepository.RemoveSelectedItemsFromCart(accountId, selectedProductVariantIds);

            // ✅ Cập nhật lại cache Redis
            await _redisCacheService.RemoveCacheAsync(GetCartKey(accountId));
        }


    }
}
