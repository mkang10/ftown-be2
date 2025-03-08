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
            var variantIds = stockUpdates.Select(s => s.VariantId).ToList();

            // Lấy toàn bộ tồn kho của các sản phẩm liên quan
            var storeStocks = await _context.StoreStocks
                .Where(s => variantIds.Contains(s.VariantId))
                .ToListAsync();

            foreach (var update in stockUpdates)
            {
                // Kiểm tra tồn kho tại cửa hàng khách chọn
                var storeStock = storeStocks.FirstOrDefault(s => s.StoreId == storeId && s.VariantId == update.VariantId);

                if (storeStock != null && storeStock.StockQuantity >= update.Quantity)
                {
                    // Nếu đủ hàng, giảm số lượng tại cửa hàng khách chọn
                    storeStock.StockQuantity -= update.Quantity;
                }
                else
                {
                    // Tìm cửa hàng có variant này nhiều nhất
                    var alternateStoreStock = storeStocks
                        .Where(s => s.VariantId == update.VariantId && s.StockQuantity >= update.Quantity)
                        .OrderByDescending(s => s.StockQuantity) // Chọn cửa hàng có nhiều hàng nhất
                        .FirstOrDefault();

                    if (alternateStoreStock == null)
                    {
                        // Nếu không tìm được cửa hàng nào có hàng, rollback transaction
                        return false;
                    }

                    // Trừ số lượng tại cửa hàng thay thế
                    alternateStoreStock.StockQuantity -= update.Quantity;
                }
            }

            // Cập nhật dữ liệu chỉ một lần
            _context.StoreStocks.UpdateRange(storeStocks);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
