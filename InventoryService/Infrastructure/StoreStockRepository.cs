using Application.DTO.Response;
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
        public async Task<bool> UpdateStockAfterOrderAsync(int storeId, List<(int VariantId, int Quantity)> stockUpdates)
        {
            foreach (var update in stockUpdates)
            {
                // Tìm bản ghi tồn kho dựa trên storeId và VariantId
                var storeStock = await _context.StoreStocks
                    .FirstOrDefaultAsync(s => s.StoreId == storeId && s.VariantId == update.VariantId);

                if (storeStock == null)
                {
                    // Nếu không tìm thấy bản ghi, trả về false
                    return false;
                }

                if (storeStock.StockQuantity < update.Quantity)
                {
                    // Nếu số lượng tồn kho không đủ để trừ
                    return false;
                }

                // Giảm tồn kho theo số lượng đặt
                storeStock.StockQuantity -= update.Quantity;
                _context.StoreStocks.Update(storeStock);
            }

            await _context.SaveChangesAsync();
            return true;
        }

    }
}
