using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IProductRepos
    {
        Task<Product> CreateAsync(Product product);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int productId);
        Task<Product?> GetByIdWithVariantsAsync(int productId);
        void Update(Product product);
        Task<int> SaveChangesAsync();

        void RemoveImage(ProductImage image);

    }
}
