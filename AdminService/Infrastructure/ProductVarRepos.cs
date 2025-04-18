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
    
        public class ProductVarRepos : IProductVarRepos
    {
            private readonly FtownContext _context;
            public ProductVarRepos(FtownContext context)
            {
                _context = context;
            }

            public async Task<ProductVariant> CreateAsync(ProductVariant variant)
            {
                await _context.ProductVariants.AddAsync(variant);
                await _context.SaveChangesAsync();
                return variant;
            }

        public async Task<int?> GetVariantIdAsync(int productId, int sizeId, int colorId)
        {
            return await _context.ProductVariants
                .Where(v => v.ProductId == productId && v.SizeId == sizeId && v.ColorId == colorId)
                .Select(v => (int?)v.VariantId)
                .FirstOrDefaultAsync();
        }

        public async Task<ProductVariant?> GetByProductSizeColorAsync(int productId, int sizeId, int colorId)
        {
            return await _context.ProductVariants
                .FirstOrDefaultAsync(v =>
                    v.ProductId == productId &&
                    v.SizeId == sizeId &&
                    v.ColorId == colorId);
        }

        public async Task<Product?> GetByIdWithVariantsAsync(int productId)
        => await _context.Products
            .Include(p => p.ProductVariants)
            .ThenInclude(e => e.Product).ThenInclude(c => c.Category)

            // navigation property
            .FirstOrDefaultAsync(p => p.ProductId == productId);
        public async Task<bool> CheckSkuExistsAsync(string sku)
           => await _context.ProductVariants.AnyAsync(v => v.Sku == sku);

      
        public async Task<int[]> GetAllVariantIdsByProductIdAsync(int productId)
            => await _context.ProductVariants.Where(v => v.ProductId == productId).Select(v => v.VariantId).ToArrayAsync();

        public async Task<ProductVariant[]> GetAllVariantsByProductIdAsync(int productId)
            => await _context.ProductVariants.Where(v => v.ProductId == productId).ToArrayAsync();
    }
    
}
