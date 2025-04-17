using Domain.Entities;
using Domain.Interfaces;
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
        }
    
}
