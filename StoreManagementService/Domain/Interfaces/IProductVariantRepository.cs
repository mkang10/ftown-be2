using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IProductVariantRepository
    {
        /// <summary>
        /// Lấy danh sách variant theo nhiều ID
        /// </summary>
        Task<IEnumerable<ProductVariant>> GetByIdsAsync(IEnumerable<int> variantIds);

        Task<ProductVariant> GetByIdAsync(int variantId);
        Task<bool> CheckSkuExistsAsync(string sku);
        Task<ProductVariant?> GetBySkuAsync(string sku);
    }
}
