using Application.DTO.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IInventoryServiceClient
    {
        Task<ProductVariantResponse?> GetProductVariantByIdAsync(int productVariantId);
        Task<List<Store>> GetAllStoresAsync();
        Task<int> GetStockQuantityAsync(int storeId, int variantId);
        Task<bool> UpdateStockAfterOrderAsync(int storeId, List<OrderDetail> orderDetails);
        Task<Dictionary<int, ProductVariantResponse?>> GetAllProductVariantsByIdsAsync(List<int> variantIds);
        Task<StoreResponse?> GetStoreByIdAsync(int storeId);
    }
}
