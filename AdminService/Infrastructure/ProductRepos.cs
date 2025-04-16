using Domain.Entities;
using Domain.Interfaces;
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
        }
    
}
