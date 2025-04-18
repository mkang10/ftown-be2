using Application.DTO.Response;
using Application.Enum;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class ProductRepository : IProductRepository
    {
        private readonly FtownContext _context;
        private readonly IRedisCacheService _cacheService;

        public ProductRepository(FtownContext context, IRedisCacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<List<Product>> GetAllProductsWithVariantsAsync()
        {
            return await _context.Products
                .Include(p => p.ProductVariants)
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Size)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Color)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.WareHousesStocks)
                .Include(p => p.Category)
                .Include(p => p.ProductImages) 
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }
		public async Task<bool> IsProductFavoriteAsync(int accountId, int productId)
		{
			return await _context.FavoriteProducts
				.AnyAsync(f => f.AccountId == accountId && f.ProductId == productId);
		}
		public async Task<ProductVariant?> GetProductVariantByIdAsync(int variantId)
        {
            return await _context.ProductVariants
                .Include(pv => pv.Product) 
                .Include(pv => pv.Size)
                .Include(pv => pv.Color)
                .FirstOrDefaultAsync(pv => pv.VariantId == variantId);
        }
        public async Task UpdateProductVariant(ProductVariant productVariant)
        {
            _context.ProductVariants.Update(productVariant);
            await _context.SaveChangesAsync();
        }
        public async Task<int> GetProductVariantStockAsync(int variantId)
        {
            return await _context.WareHousesStocks
                .Where(ss => ss.VariantId == variantId)
                .SumAsync(ss => ss.StockQuantity);
        }
        public async Task<List<Product>> GetPagedProductsWithVariantsAsync(int page, int pageSize)
        {
            return await _context.Products
				.Where(p => p.Status == ProductStatus.Online.ToString()
		                   || p.Status == ProductStatus.Both.ToString() )
				.Include(p => p.ProductVariants)
                    .ThenInclude(pv => pv.Color)
                .Include(p => p.Category)
                .Include(p => p.ProductImages) 
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<ProductVariant>> GetProductVariantsByIdsAsync(List<int> variantIds)
        {
            return await _context.ProductVariants
                .Include(pv => pv.Product) 
                .Include(pv => pv.Size)
                .Include(pv => pv.Color)
                .Where(pv => variantIds.Contains(pv.VariantId))
                .ToListAsync();
        }

        
        public async Task<Dictionary<int, int>> GetProductVariantsStockAsync(List<int> variantIds)
        {
            return await _context.WareHousesStocks
                .Where(ss => variantIds.Contains(ss.VariantId)) // Chỉ lấy các VariantId được yêu cầu
                .GroupBy(ss => ss.VariantId) // Gom nhóm theo VariantId
                .Select(g => new { VariantId = g.Key, StockQuantity = g.Sum(ss => ss.StockQuantity) }) // Tổng tồn kho
                .ToDictionaryAsync(x => x.VariantId, x => x.StockQuantity);
        }

        public async Task<ProductVariant?> GetProductVariantByDetailsAsync(int productId, string size, string color)
        {
            return await _context.ProductVariants
                .Include(pv => pv.Size)
                .Include(pv => pv.Color)
                .Where(pv => pv.ProductId == productId &&
                             pv.Size != null && pv.Size.SizeName.ToLower() == size.ToLower() &&
                             pv.Color != null && pv.Color.ColorCode.ToLower() == color.ToLower())
                .FirstOrDefaultAsync();
        }

        public async Task AddProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }
        public async Task AddProductsAsync(IEnumerable<Product> products)
        {
            _context.Products.AddRange(products);
            await _context.SaveChangesAsync();
        }

        public async Task AddProductVariantsAsync(List<ProductVariant> variants)
        {
            _context.ProductVariants.AddRange(variants);
            await _context.SaveChangesAsync();
        }

        public async Task AddProductImagesAsync(List<ProductImage> images)
        {
            _context.ProductImages.AddRange(images);
            await _context.SaveChangesAsync();
        }

		public async Task AddFavoriteAsync(int accountId, int productId)
		{
			bool exists = await _context.FavoriteProducts
				.AnyAsync(f => f.AccountId == accountId && f.ProductId == productId);

			if (!exists)
			{
				_context.FavoriteProducts.Add(new FavoriteProduct
				{
					AccountId = accountId,
					ProductId = productId,
					CreatedAt = DateTime.UtcNow
				});

				await _context.SaveChangesAsync();
			}
		}

		public async Task RemoveFavoriteAsync(int accountId, int productId)
		{
			var favorite = await _context.FavoriteProducts
				.FirstOrDefaultAsync(f => f.AccountId == accountId && f.ProductId == productId);

			if (favorite != null)
			{
				_context.FavoriteProducts.Remove(favorite);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<List<Product>> GetFavoritePagedProductsAsync(int accountId, int page, int pageSize)
		{
			var query = from product in _context.Products
						join favorite in _context.FavoriteProducts
							on product.ProductId equals favorite.ProductId
						where favorite.AccountId == accountId
						   && (product.Status == ProductStatus.Online.ToString()
							   || product.Status == ProductStatus.Both.ToString())
						orderby favorite.CreatedAt descending // ✅ Sắp xếp mới nhất trước
						select product;

			return await query
				.Include(p => p.ProductVariants)
				.Include(p => p.Category)
				.Include(p => p.ProductImages)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();
		}

        public async Task<List<Product>> GetProductsByCategoryNameAsync(string categoryName)
        {
            return await _context.Products
                .Where(p =>
                    (p.Status == ProductStatus.Online.ToString() ||
                     p.Status == ProductStatus.Both.ToString()) &&
                    p.Category.Name.ToLower() == categoryName.ToLower())
                .Include(p => p.Category)
                .Include(p => p.ProductVariants)
                .Include(p => p.ProductImages)
                .ToListAsync();
        }

        public async Task<List<Order>> GetCompletedOrdersWithDetailsAsync(DateTime? from, DateTime? to)
        {
            var query = _context.Orders
                .Where(o => o.Status == "completed" && o.CreatedDate.HasValue)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                            .ThenInclude(p => p.Category)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                            .ThenInclude(p => p.ProductImages)
                .Include(o => o.OrderDetails) // 🆕 Thêm dòng này để lấy màu
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(pv => pv.Color)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(o => o.CreatedDate.Value.Date >= from.Value.Date);

            if (to.HasValue)
                query = query.Where(o => o.CreatedDate.Value.Date <= to.Value.Date);

            return await query.ToListAsync();
        }


    }
}
