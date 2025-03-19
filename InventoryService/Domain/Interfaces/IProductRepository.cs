using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllProductsWithVariantsAsync();
        Task<Product?> GetProductByIdAsync(int productId);
        Task<ProductVariant?> GetProductVariantByIdAsync(int variantId);
        Task<int> GetProductVariantStockAsync(int variantId);
        //Task UpdateProductVariantAsync(ProductVariant productVariant);
        Task<List<Product>> GetPagedProductsWithVariantsAsync(int page, int pageSize);
        Task<List<ProductVariant>> GetProductVariantsByIdsAsync(List<int> variantIds);
        Task<Dictionary<int, int>> GetProductVariantsStockAsync(List<int> variantIds);
        Task<ProductVariant?> GetProductVariantByDetailsAsync(int productId, string size, string color);
    }
}
