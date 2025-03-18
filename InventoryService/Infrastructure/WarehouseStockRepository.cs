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
                .FirstOrDefaultAsync(ss => ss.WarehouseId == warehouseId && ss.VariantId == variantId);
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
                .Include(ss => ss.Warehouse)
                .Where(ss => ss.VariantId == variantId)
                .ToListAsync();
        }
        public async Task<bool> UpdateStockAfterOrderAsync(int warehouseId, List<(int VariantId, int Quantity)> stockUpdates)
        {
            var variantIds = stockUpdates.Select(s => s.VariantId).ToList();

            // Lấy toàn bộ tồn kho của các sản phẩm liên quan
            var warehouseStocks = await _context.WareHousesStocks
                .Where(s => variantIds.Contains(s.VariantId))
                .ToListAsync();

            foreach (var update in stockUpdates)
            {
                // Kiểm tra tồn kho tại cửa hàng khách chọn
                var storeStock = warehouseStocks.FirstOrDefault(s => s.WarehouseId == warehouseId && s.VariantId == update.VariantId);

                if (storeStock != null && storeStock.StockQuantity >= update.Quantity)
                {
                    // Nếu đủ hàng, giảm số lượng tại cửa hàng khách chọn
                    storeStock.StockQuantity -= update.Quantity;
                }
                else
                {
                    // Tìm cửa hàng có variant này nhiều nhất
                    var alternateWarehouseStock = warehouseStocks
                        .Where(s => s.VariantId == update.VariantId && s.StockQuantity >= update.Quantity)
                        .OrderByDescending(s => s.StockQuantity) // Chọn cửa hàng có nhiều hàng nhất
                        .FirstOrDefault();

                    if (alternateWarehouseStock == null)
                    {
                        // Nếu không tìm được cửa hàng nào có hàng, rollback transaction
                        return false;
                    }

                    // Trừ số lượng tại cửa hàng thay thế
                    alternateWarehouseStock.StockQuantity -= update.Quantity;
                }
            }

            // Cập nhật dữ liệu chỉ một lần
            _context.WareHousesStocks.UpdateRange(warehouseStocks);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
