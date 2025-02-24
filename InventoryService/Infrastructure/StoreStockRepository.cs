using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class StoreStockRepository : IStoreStockRepository
    {
        private readonly FtownContext _context;

        public StoreStockRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<int> GetStockQuantityAsync(int storeId, int variantId)
        {
            var storeStock = await _context.StoreStocks
                .FirstOrDefaultAsync(ss => ss.StoreId == storeId && ss.VariantId == variantId);
            return storeStock?.StockQuantity ?? 0;
        }

        public async Task<int> GetTotalStockByVariantAsync(int variantId)
        {
            int total = await _context.StoreStocks
                .Where(ss => ss.VariantId == variantId)
                .SumAsync(ss => (int?)ss.StockQuantity) ?? 0;
            return total;
        }

        public async Task<List<StoreStock>> GetStoreStocksByVariantAsync(int variantId)
        {
            return await _context.StoreStocks
                .Include(ss => ss.Store)
                .Where(ss => ss.VariantId == variantId)
                .ToListAsync();
        }
    }
}
