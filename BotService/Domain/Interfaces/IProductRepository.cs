using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IProductRepository
    {
        /// <summary>
        /// Lấy tất cả variants theo bộ lọc: occasion, style, size và trạng thái active.
        /// </summary>
        Task<List<ProductVariant>> GetVariantsByFiltersAsync(
            string occasion,
            string style,
            int sizeId,
            CancellationToken ct = default);
    }
}
