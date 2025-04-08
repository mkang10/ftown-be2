using Application.DTO.Response;
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
                .Include(p => p.ProductImages) // 👈 Thêm Include để lấy danh sách ảnh
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        public async Task<ProductVariant?> GetProductVariantByIdAsync(int variantId)
        {
            return await _context.ProductVariants
                .Include(pv => pv.Product) // Lấy thông tin Product cha
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
                .Include(p => p.ProductVariants)
                .Include(p => p.Category)
                .Include(p => p.ProductImages) // Lấy luôn các ảnh của sản phẩm
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<ProductVariant>> GetProductVariantsByIdsAsync(List<int> variantIds)
        {
            return await _context.ProductVariants
                .Include(pv => pv.Product) // Lấy thông tin Product cha
                .Include(pv => pv.Size)
                .Include(pv => pv.Color)
                .Where(pv => variantIds.Contains(pv.VariantId))
                .ToListAsync();
        }

        // 🔥 Cập nhật truy vấn lấy tồn kho từ StoreStock
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

        //public async Task AddProduct(Product product)
        //{
        //    _context.Products.Add(product);
        //    await _context.SaveChangesAsync();

        //    var productDto = _mapper.Map<ProductListResponse>(product);

        //    // Index vào Elasticsearch
        //    await _elasticsearchService.IndexProductAsync(productDto);
        //}
    }
}
