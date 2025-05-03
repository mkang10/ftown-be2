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
    public class WareHousesStockRepository : IWareHousesStockRepository
    {
        private readonly FtownContext _context;

        public WareHousesStockRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<int> GetStockQuantityAsync(int warehouseId, int variantId)
        {
            var warehouseStock = await _context.WareHousesStocks
                .FirstOrDefaultAsync(ss => ss.WareHouseId == warehouseId && ss.VariantId == variantId);
            return warehouseStock?.StockQuantity ?? 0;
        }

        public async Task<int> GetTotalStockByVariantAsync(int variantId)
        {
            int total = await _context.WareHousesStocks
                .Where(ss => ss.VariantId == variantId)
                .SumAsync(ss => (int?)ss.StockQuantity) ?? 0;
            return total;
        }

        public async Task<List<WareHousesStock>> GetWareHouseStocksByVariantAsync(int variantId)
        {
            return await _context.WareHousesStocks
                .Include(ss => ss.WareHouse)
                .Where(ss => ss.VariantId == variantId)
                .ToListAsync();
        }
        public async Task<bool> UpdateStockAfterOrderAsync(int warehouseId, List<(int VariantId, int Quantity)> stockUpdates)
        {
            // Lấy danh sách VariantId cần trừ tồn kho
            var variantIds = stockUpdates.Select(s => s.VariantId).ToList();

            // Lấy tồn kho tại kho duy nhất
            var warehouseStocks = await _context.WareHousesStocks
                .Where(s => s.WareHouseId == warehouseId && variantIds.Contains(s.VariantId))
                .ToListAsync();

            // Kiểm tra tất cả mặt hàng có đủ tồn kho không
            foreach (var update in stockUpdates)
            {
                var stockItem = warehouseStocks.FirstOrDefault(s => s.VariantId == update.VariantId);

                if (stockItem == null || stockItem.StockQuantity < update.Quantity)
                {
                    return false; // Không đủ hàng, huỷ luôn
                }
            }

            // Nếu tất cả đều đủ hàng thì tiến hành trừ tồn kho
            foreach (var update in stockUpdates)
            {
                var stockItem = warehouseStocks.First(s => s.VariantId == update.VariantId);
                stockItem.StockQuantity -= update.Quantity;
            }

            _context.WareHousesStocks.UpdateRange(warehouseStocks);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RestoreStockAfterCancelAsync(int warehouseId, List<(int VariantId, int Quantity)> stockUpdates)
        {
            var variantIds = stockUpdates.Select(s => s.VariantId).ToList();

            var warehouseStocks = await _context.WareHousesStocks
                .Where(s => s.WareHouseId == warehouseId && variantIds.Contains(s.VariantId))
                .ToListAsync();

            foreach (var update in stockUpdates)
            {
                var stockItem = warehouseStocks.FirstOrDefault(s => s.VariantId == update.VariantId);
                if (stockItem == null)
                {
                    // Nếu chưa có thì tạo mới luôn
                    stockItem = new WareHousesStock
                    {
                        WareHouseId = warehouseId,
                        VariantId = update.VariantId,
                        StockQuantity = 0
                    };
                    _context.WareHousesStocks.Add(stockItem);
                }

                stockItem.StockQuantity += update.Quantity;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
