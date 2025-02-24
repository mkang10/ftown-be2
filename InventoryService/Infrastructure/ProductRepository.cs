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

        public ProductRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllProductsWithVariantsAsync()
        {
            return await _context.Products
                .Include(p => p.ProductVariants)  // Lấy biến thể sản phẩm
                .Include(p => p.Category)        // Lấy danh mục sản phẩm
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.ProductVariants)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }
        public async Task<ProductVariant?> GetProductVariantByIdAsync(int variantId)
        {
            return await _context.ProductVariants
                .Include(pv => pv.Product) // Lấy thông tin Product cha
                .FirstOrDefaultAsync(pv => pv.VariantId == variantId);
        }
    }
}
