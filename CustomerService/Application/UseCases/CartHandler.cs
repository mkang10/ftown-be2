using Application.DTO.Request;
using Application.DTO.Response;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class CartHandler
    {
        private readonly ICartRepository _cartRepository;
        private readonly IInventoryServiceClient _inventoryServiceClient;
        private readonly IMapper _mapper;

        public CartHandler(ICartRepository cartRepository, IInventoryServiceClient inventoryServiceClient, IMapper mapper)
        {
            _cartRepository = cartRepository;
            _inventoryServiceClient = inventoryServiceClient;
            _mapper = mapper;
        }
        public async Task<List<CartItemResponse>> GetShoppingCart(int accountId)
        {
            // 1️⃣ Kiểm tra giỏ hàng trong Redis trước
            var cart = await _cartRepository.GetCartAsync(accountId);

            if (cart == null || !cart.Any())
            {
                Console.WriteLine("⏳ Không tìm thấy giỏ hàng trong Redis, lấy từ Database...");

                // 2️⃣ Lấy giỏ hàng từ Database nếu Redis không có
                cart = await _cartRepository.GetCartFromDatabase(accountId);

                if (cart.Any())
                {
                    // 3️⃣ Lưu giỏ hàng từ Database vào Redis để dùng cho lần sau
                    await _cartRepository.UpdateCartAsync(accountId, cart);
                }
            }

            var cartItemResponses = new List<CartItemResponse>();

            foreach (var item in cart)
            {
                // 4️⃣ Lấy thông tin ProductVariant từ InventoryService
                var productVariant = await _inventoryServiceClient.GetProductVariantByIdAsync(item.ProductVariantId);
                if (productVariant != null)
                {
                    cartItemResponses.Add(new CartItemResponse
                    {
                        ProductVariantId = item.ProductVariantId,
                        ProductName = productVariant.ProductName,
                        ImagePath = productVariant.ImagePath,
                        Size = productVariant.Size,
                        Color = productVariant.Color,
                        Quantity = item.Quantity,
                        Price = productVariant.Price
                    });
                }
            }

            return cartItemResponses;
        }



        public async Task AddCartItem(int accountId, AddToCartRequest cartItemDto)
        {
            var cartItem = new CartItem
            {
                ProductVariantId = cartItemDto.ProductVariantId,
                Quantity = cartItemDto.Quantity
            };

            await _cartRepository.AddToCartAsync(accountId, cartItem);
        }

        public async Task RemoveCartItem(int accountId, int productVariantId)
        {
            await _cartRepository.RemoveFromCartAsync(accountId, productVariantId);
        }

        public async Task ClearCart(int accountId)
        {
            await _cartRepository.ClearCartAsync(accountId);
        }

        public async Task SyncCartToDatabase(int accountId)
        {
            var cart = await _cartRepository.GetCartAsync(accountId);
            if (cart == null || !cart.Any()) return;

            await _cartRepository.SyncCartToDatabase(accountId, cart);
        }
        public async Task ClearCartAfterOrderAsync(int accountId)
        {
            // Xóa giỏ hàng trong DB
            await _cartRepository.ClearCartInDatabase(accountId);
            // Xóa giỏ hàng trong Redis
            await _cartRepository.ClearCartAsync(accountId);
        }
    }
}
