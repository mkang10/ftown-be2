using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class CartRepository : ICartRepository
    {
        private readonly IDistributedCache _cache;
        private readonly FtownContext _dbContext;
        private readonly string CART_PREFIX = "cart:";
        private readonly IConnectionMultiplexer _redis;
        public CartRepository(IDistributedCache cache, FtownContext dbContext, IConnectionMultiplexer redis)
        {
            _cache = cache;
            _dbContext = dbContext;
            _redis = redis;
        }

        private string GetCartKey(int accountId) => $"{CART_PREFIX}{accountId}";
        public async Task<List<CartItem>> GetCartFromDatabase(int accountId)
        {
            var shoppingCart = await _dbContext.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.AccountId == accountId);

            if (shoppingCart == null)
            {
                return new List<CartItem>(); // Trả về danh sách rỗng nếu giỏ hàng không tồn tại
            }

            return shoppingCart.CartItems.ToList();
        }

        public async Task<List<CartItem>> GetCartAsync(int accountId)
        {
            var cartData = await _cache.GetStringAsync(GetCartKey(accountId));
            return cartData != null
            ? System.Text.Json.JsonSerializer.Deserialize<List<CartItem>>(cartData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })
            : new List<CartItem>();
        }

        public async Task SyncCartToDatabase(int accountId, List<CartItem> cartItems)
        {
            // Kiểm tra xem ShoppingCart đã tồn tại chưa
            var shoppingCart = await _dbContext.ShoppingCarts
                .FirstOrDefaultAsync(c => c.AccountId == accountId);

            // Nếu ShoppingCart chưa tồn tại, tạo mới
            if (shoppingCart == null)
            {
                shoppingCart = new ShoppingCart
                {
                    AccountId = accountId,
                    CreatedDate = DateTime.UtcNow
                };

                _dbContext.ShoppingCarts.Add(shoppingCart);
                await _dbContext.SaveChangesAsync(); // Lưu ngay để có CartId hợp lệ
            }

            // Xóa giỏ hàng cũ trong DB để tránh trùng lặp
            var existingCartItems = _dbContext.CartItems
                .Where(c => c.CartId == shoppingCart.CartId);
            _dbContext.CartItems.RemoveRange(existingCartItems);
            await _dbContext.SaveChangesAsync();

            // Thêm giỏ hàng từ Redis vào Database
            foreach (var item in cartItems)
            {
                _dbContext.CartItems.Add(new CartItem
                {
                    CartId = shoppingCart.CartId, // Sử dụng CartId hợp lệ
                    ProductVariantId = item.ProductVariantId,
                    Quantity = item.Quantity
                });
            }

            await _dbContext.SaveChangesAsync();

            // Xóa giỏ hàng trên Redis sau khi đồng bộ
            await ClearCartAsync(accountId);
        }
        public async Task AddToCartAsync(int accountId, CartItem cartItem)
        {
            var cart = await GetCartAsync(accountId);
            var existingItem = cart.Find(c => c.ProductVariantId == cartItem.ProductVariantId);

            if (existingItem != null)
            {
                existingItem.Quantity += cartItem.Quantity;
            }
            else
            {
                cart.Add(cartItem);
            }

            await UpdateCartAsync(accountId, cart);
        }

        public async Task UpdateCartAsync(int accountId, List<CartItem> cart)
        {
            if (cart == null || !cart.Any())
            {
                // Nếu giỏ hàng trống, xóa key khỏi Redis để tránh dữ liệu dư thừa
                await _cache.RemoveAsync(GetCartKey(accountId));
                return;
            }

            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            var cartData = JsonConvert.SerializeObject(cart, jsonSettings);
            await _cache.SetStringAsync(GetCartKey(accountId), cartData, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) // Giữ giỏ hàng trong Redis 1 giờ
            });
        }

        public async Task RemoveFromCartAsync(int accountId, int productVariantId)
        {
            // 1️⃣ Lấy giỏ hàng từ Redis
            var cart = await GetCartAsync(accountId);
            if (cart == null || !cart.Any()) return;

            // 2️⃣ Tìm sản phẩm trong giỏ hàng
            var cartItem = cart.FirstOrDefault(c => c.ProductVariantId == productVariantId);

            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    // 🔹 Nếu số lượng > 1, giảm số lượng
                    cartItem.Quantity--;
                }
                else
                {
                    // 🔹 Nếu số lượng = 1, xóa khỏi giỏ hàng
                    cart.Remove(cartItem);
                }

                if (cart.Any())
                {
                    // 3️⃣ Nếu còn sản phẩm, cập nhật lại giỏ hàng trên Redis
                    await UpdateCartAsync(accountId, cart);
                }
                else
                {
                    // 4️⃣ Nếu giỏ hàng trống, xóa Redis và cập nhật database
                    await _cache.RemoveAsync(GetCartKey(accountId));
                    await ClearCartInDatabase(accountId);
                }
            }
        }


        public async Task ClearCartAsync(int accountId)
        {
            var cacheKey = GetCartKey(accountId);
            await _cache.RemoveAsync(cacheKey);
        }

        public async Task ClearCartInDatabase(int accountId)
        {
            var shoppingCart = await _dbContext.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.AccountId == accountId);

            if (shoppingCart != null)
            {
                _dbContext.CartItems.RemoveRange(shoppingCart.CartItems);
                _dbContext.ShoppingCarts.Remove(shoppingCart);
                await _dbContext.SaveChangesAsync();
            }
        }

    }

}
