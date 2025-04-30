using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using System;
using Domain.Interfaces;

namespace Infrastructure.Repositories
{
    public class WarehouseStockRepository : IWarehouseStockRepos
    {
        private readonly FtownContext _context;
        private readonly IProductVarRepos _varRepos;

        public WarehouseStockRepository(IProductVarRepos varRepos, FtownContext context)
        {
            _context = context;
            _varRepos = varRepos;
        }

        public async Task<WareHousesStock?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.WareHousesStocks
                .Include(ws => ws.Variant)
                    .ThenInclude(v => v.Product)
                .Include(ws => ws.Variant)
                    .ThenInclude(v => v.Size)
                .Include(ws => ws.Variant)
                    .ThenInclude(v => v.Color)
                .Include(ws => ws.WareHouse)
               .Include(ws => ws.WareHouseStockAudits)
            .ThenInclude(a => a.ChangedByNavigation)


                .FirstOrDefaultAsync(ws => ws.WareHouseStockId == id);
        }

        public async Task<IEnumerable<WareHousesStock>> GetByWarehouseIdAsync(int warehouseId)
        {
            return await _context.WareHousesStocks
                .Where(ws => ws.WareHouseId == warehouseId)
                .Include(ws => ws.Variant).ThenInclude(v => v.Product)
                .Include(ws => ws.Variant).ThenInclude(v => v.Size)
                .Include(ws => ws.Variant).ThenInclude(v => v.Color)
                .Include(ws => ws.WareHouse)
                .Include(ws => ws.WareHouseStockAudits)
                .ToListAsync();
        }

        public async Task<bool> HasStockAsync(int productId, int sizeId, int colorId)
        {
            var variantId = await _varRepos.GetVariantIdAsync(productId, sizeId, colorId);
            if (variantId == null)
                return false;

            return await _context.WareHousesStocks
                .AnyAsync(ws => ws.VariantId == variantId && ws.StockQuantity > 0);
        }
    }
}
