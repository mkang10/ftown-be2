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
        Task<int> GetStockQuantityAsync(int storeId, int variantId);
        Task<bool> UpdateStockAfterOrderAsync(int warehouseId, List<OrderDetail> orderDetails);
        Task<Dictionary<int, ProductVariantResponse?>> GetAllProductVariantsByIdsAsync(List<int> variantIds);
    }
}
