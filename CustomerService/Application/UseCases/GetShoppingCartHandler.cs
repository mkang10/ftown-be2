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
            // Kiểm tra giỏ hàng trong Redis
            var cart = await _redisCacheService.GetCacheAsync<List<CartItem>>(cartKey) ?? new List<CartItem>();

            // Nếu cache trống, tải từ DB và cập nhật cache
            if (!cart.Any())
            {
                Console.WriteLine("⏳ Cache miss: Lấy giỏ hàng từ Database...");
                cart = await _cartRepository.GetCartFromDatabaseAsync(accountId);
                if (cart.Any())
                {
                    await _redisCacheService.SetCacheAsync(cartKey, cart, TimeSpan.FromMinutes(30));
                }
            }

            // Nếu vẫn trống, trả về giỏ hàng rỗng
            if (!cart.Any())
            {
                return new ResponseDTO<List<CartItemResponse>>(new List<CartItemResponse>(), true, "Giỏ hàng trống.");
            }

            // Ánh xạ sang response
            var cartItemResponses = _mapper.Map<List<CartItemResponse>>(cart);

            // Lấy thông tin sản phẩm song song từ InventoryService
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
            //Lấy ProductVariant từ InventoryService dựa trên ProductId, Size, Color
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

            //Lấy giỏ hàng từ Redis; nếu không có thì tải từ DB
            var cart = await _redisCacheService.GetCacheAsync<List<CartItem>>(cartKey);
            if (cart == null)
            {
                cart = await _cartRepository.GetCartFromDatabaseAsync(accountId) ?? new List<CartItem>();
            }

            //Cập nhật giỏ hàng trong bộ nhớ
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

            // Cập nhật dữ liệu vào DB qua repository
            await _cartRepository.AddToCartAsync(accountId, new CartItem
            {
                ProductVariantId = productVariant.VariantId,
                Quantity = cartItemDto.Quantity
            });

            // Cập nhật cache Redis với giỏ hàng mới nhất
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

        public async Task<ResponseDTO<bool>> ChangeCartItemQuantity(int accountId, ChangeCartItemQuantityRequest request)
        {
            var cartKey = GetCartKey(accountId);

            var cart = await _redisCacheService.GetCacheAsync<List<CartItem>>(cartKey);
            if (cart == null)
            {
                cart = await _cartRepository.GetCartFromDatabaseAsync(accountId) ?? new List<CartItem>();
            }

            var item = cart.FirstOrDefault(c => c.ProductVariantId == request.ProductVariantId);
            if (item == null)
            {
                return new ResponseDTO<bool>(false, false, "Sản phẩm không có trong giỏ hàng.");
            }

            var newQuantity = item.Quantity + request.QuantityChange;

            // 👉 Nếu số lượng mới <= 0 → xoá khỏi giỏ
            if (newQuantity <= 0)
            {
                cart.Remove(item);
                await _cartRepository.RemoveFromCartAsync(accountId, request.ProductVariantId);
                await _redisCacheService.SetCacheAsync(cartKey, cart, TimeSpan.FromMinutes(30));
                return new ResponseDTO<bool>(true, true, "Sản phẩm đã được xóa khỏi giỏ hàng.");
            }

            // Kiểm tra tồn kho
            var variant = await _inventoryServiceClient.GetProductVariantById(request.ProductVariantId);
            if (variant == null)
            {
                return new ResponseDTO<bool>(false, false, "Sản phẩm không tồn tại.");
            }

            if (newQuantity > variant.StockQuantity)
            {
                return new ResponseDTO<bool>(false, false, "Số lượng vượt quá tồn kho.");
            }

            // Cập nhật số lượng mới
            item.Quantity = newQuantity;
            await _cartRepository.UpdateCartItemQuantityAsync(accountId, request.ProductVariantId, newQuantity);
            await _redisCacheService.SetCacheAsync(cartKey, cart, TimeSpan.FromMinutes(30));

            return new ResponseDTO<bool>(true, true, "Cập nhật số lượng sản phẩm thành công.");
        }

    }
}
