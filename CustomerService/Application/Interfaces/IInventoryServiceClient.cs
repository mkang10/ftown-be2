using Application.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IInventoryServiceClient
    {
        Task<List<ProductResponse>?> GetAllProductsAsync();
        Task<ProductDetailResponse?> GetProductByIdAsync(int productId);
        Task<ProductVariantResponse?> GetProductVariantByIdAsync(int productVariantId);
        Task<ProductVariantResponse?> GetProductVariantById(int variantId);
        Task<ProductVariantResponse?> GetProductVariantByDetails(int productId, string size, string color);
        Task<List<ProductResponse>?> GetProductsByStyleNameAsync(string styleName, int page, int pageSize);
    }
}
