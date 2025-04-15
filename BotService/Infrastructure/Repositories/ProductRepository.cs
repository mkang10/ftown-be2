using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly FtownContext _ctx;
        public ProductRepository(FtownContext ctx) => _ctx = ctx;

        public async Task<List<ProductVariant>> GetVariantsByFiltersAsync(
            string occasion,
            string style,
            int sizeId,
            CancellationToken ct = default)
        {
            return await _ctx.ProductVariants
                .Include(v => v.Product).ThenInclude(p => p.Category)
                .Include(v => v.Color)
                .Include(v => v.Size)
                .Where(v =>
                    v.SizeId == sizeId &&
                    v.Product.Occasion == occasion &&
                    v.Product.Style == style &&
                    v.Product.Status == "Active")
                .ToListAsync(ct);
        }
    }
}
