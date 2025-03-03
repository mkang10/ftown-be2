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
        Task<ProductVariant?> GetProductVariantByIdAsync(int productVariantId);
        Task<List<Store>> GetAllStoresAsync();
        Task<int> GetStockQuantityAsync(int storeId, int variantId);
        Task<bool> UpdateStockAfterOrderAsync(int storeId, List<OrderDetail> orderDetails);
    }
}
