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
        Task<bool> IsProductFavoriteAsync(int accountId, int productId);

		Task<ProductVariant?> GetProductVariantByIdAsync(int variantId);
        Task<int> GetProductVariantStockAsync(int variantId);
        Task<List<Product>> GetPagedProductsWithVariantsAsync(int page, int pageSize);
        Task<List<ProductVariant>> GetProductVariantsByIdsAsync(List<int> variantIds);
        Task<Dictionary<int, int>> GetProductVariantsStockAsync(List<int> variantIds);
        Task<ProductVariant?> GetProductVariantByDetailsAsync(int productId, string size, string color);
        Task AddProductAsync(Product product);
        Task AddProductsAsync(IEnumerable<Product> products);
        Task AddProductVariantsAsync(List<ProductVariant> variants);
        Task AddProductImagesAsync(List<ProductImage> images);
		Task AddFavoriteAsync(int accountId, int productId);
		Task RemoveFavoriteAsync(int accountId, int productId);
		Task<List<Product>> GetFavoritePagedProductsAsync(int accountId, int page, int pageSize);
	}
}
