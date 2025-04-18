﻿using Domain.Entities;
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

        public Task<Product?> GetByIdAsync(int productId)
          => _context.Products.FindAsync(productId).AsTask();

       
        public async Task<Product?> GetByIdWithVariantsAsync(int productId)
            => await _context.Products.Include(p => p.ProductVariants)
            .Include(c => c.Category)
            .Include(p => p.ProductImages)
                                       .FirstOrDefaultAsync(p => p.ProductId == productId);
    }
    
}
