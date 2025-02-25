using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ICartRepository
    {
        Task<List<CartItem>> GetCartAsync(int accountId);
        Task<List<CartItem>> GetCartFromDatabase(int accountId);
        Task UpdateCartAsync(int accountId, List<CartItem> cart);

        Task AddToCartAsync(int accountId, CartItem cartItem);
        Task RemoveFromCartAsync(int accountId, int productVariantId);
        Task ClearCartAsync(int accountId);
        Task SyncCartToDatabase(int accountId, List<CartItem> cartItems);
        //Task ClearRedisCache();
        Task ClearCartInDatabase(int accountId);


    }
}
