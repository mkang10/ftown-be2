// Infrastructure/Repositories/StockRepos.cs
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class StockRepos : IStockRepos
    {
        private readonly FtownContext _db;

        public StockRepos(FtownContext db)
        {
            _db = db;
        }

        public async Task<WareHousesStock> GetByWarehouseAndVariantAsync(int warehouseId, int variantId)
        {
            return await _db.WareHousesStocks
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s => s.WareHouseId == warehouseId
                                                   && s.VariantId == variantId);
        }
    }
}
