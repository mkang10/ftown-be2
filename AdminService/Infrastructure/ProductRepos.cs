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
   
        public class ProductRepos : IProductRepos
    {
            private readonly FtownContext _context;
            public ProductRepos(FtownContext context)
            {
                _context = context;
            }

            public async Task<Product> CreateAsync(Product product)
            {
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();
                return product;
            }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.ProductImages)
                .AsSplitQuery() // ✅ Thêm dòng này để tránh lỗi tracking duplicate entity
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }




        public async Task<Product?> GetByIdWithVariantsAsync(int productId)
            => await _context.Products.Include(p => p.ProductVariants)
            .Include(c => c.Category)
            .Include(p => p.ProductImages)
                                       .FirstOrDefaultAsync(p => p.ProductId == productId);

        public void Update(Product product) => _context.Products.Update(product);

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

        public void RemoveImage(ProductImage image)
        {
            _context.ProductImages.Remove(image);
        }
    }
    
}
