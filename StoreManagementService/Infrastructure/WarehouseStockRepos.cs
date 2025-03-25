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
    public class WareHouseStockRepos : IWareHouseStockRepos
    {
        private readonly FtownContext _context;

        public WareHouseStockRepos(FtownContext context)
        {
            _context = context;
        }

        public async Task UpdateWarehouseStockAsync(Import import, int staffId)
        {
            // Duyệt qua từng ImportDetail trong Import
            foreach (var importDetail in import.ImportDetails)
            {
                int variantId = importDetail.ProductVariantId;
                foreach (var storeDetail in importDetail.ImportStoreDetails)
                {
                    int allocatedQty = storeDetail.AllocatedQuantity;
                    int warehouseId = (int)storeDetail.WarehouseId;

                    // Tìm WareHousesStock theo WarehouseId và VariantId
                    var wareHouseStock = await _context.WareHousesStocks
                        .FirstOrDefaultAsync(ws => ws.WareHouseId == warehouseId && ws.VariantId == variantId);

                    string auditAction = "";
                    if (wareHouseStock == null)
                    {
                        // Chưa có: tạo bản ghi mới
                        wareHouseStock = new WareHousesStock
                        {
                            WareHouseId = warehouseId,
                            VariantId = variantId,
                            StockQuantity = allocatedQty
                        };
                        _context.WareHousesStocks.Add(wareHouseStock);
                        // Lưu ngay để nhận WareHouseStockId
                        await _context.SaveChangesAsync();
                        auditAction = "CREATE";
                    }
                    else
                    {
                        // Đã có: tăng số lượng tồn kho
                        wareHouseStock.StockQuantity += allocatedQty;
                        auditAction = "INCREASE";
                    }

                    // Tạo WareHouseStockAudit cho giao dịch cập nhật tồn kho
                    var stockAudit = new WareHouseStockAudit
                    {
                        WareHouseStockId = wareHouseStock.WareHouseStockId, // Đã được gán sau SaveChanges nếu là bản ghi mới
                        Action = auditAction,
                        QuantityChange = allocatedQty,
                        ActionDate = DateTime.Now,
                        ChangedBy = staffId,
                        Note = $"Updated via Import Done. ImportDetailId: {importDetail.ImportDetailId}, ImportStoreId: {storeDetail.ImportStoreId}"
                    };
                    _context.WareHouseStockAudits.Add(stockAudit);
                }
            }

            // Cuối cùng lưu lại các bản ghi audit (và các cập nhật tồn kho nếu có bản ghi cũ)
            await _context.SaveChangesAsync();
        }


        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

