using Domain.DTO.Response;
using Domain.DTO.Response.Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
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
        private readonly IStaffDetailRepository _staffDetail;

        public WareHouseStockRepos(FtownContext context, IStaffDetailRepository staffDetail)
        {
            _context = context;
            _staffDetail = staffDetail;
        }
        public async Task<IEnumerable<WareHousesStock>> GetByWarehouseAsync(int warehouseId)
            => await _context.WareHousesStocks
                .Where(s => s.WareHouseId == warehouseId)
                .ToListAsync();

        public async Task<WareHousesStock?> GetByWarehouseAndVariantAsync(int warehouseId, int variantId)
            => await _context.WareHousesStocks
                .FirstOrDefaultAsync(s => s.WareHouseId == warehouseId && s.VariantId == variantId);

        public async Task<IEnumerable<WareHousesStock>> GetAllByVariantAsync(int variantId)
            => await _context.WareHousesStocks
                .Where(s => s.VariantId == variantId)
                .ToListAsync();

        public Task UpdateAsync(WareHousesStock stock)
        {
            _context.WareHousesStocks.Update(stock);
            return Task.CompletedTask;
        }
       public async Task UpdateWarehouseStockAsync(
    Import import,
    int staffId,
    List<int> confirmedStoreDetailIds)
{
            var accountId = await _staffDetail.GetAccountIdByStaffIdAsync(staffId);
            // Chỉ lấy những storeDetail vừa xác nhận
            var detailStorePairs = import.ImportDetails
        .SelectMany(d => d.ImportStoreDetails
            .Where(sd =>
                sd.ActualReceivedQuantity.HasValue &&
                sd.WarehouseId.HasValue &&
                confirmedStoreDetailIds.Contains(sd.ImportStoreId)
            )
            .Select(sd => new { Detail = d, Store = sd }))
        .ToList();

    using var tx = await _context.Database.BeginTransactionAsync();
    try
    {
                foreach (var pair in detailStorePairs)
                {
                    var detail = pair.Detail;
                    var store = pair.Store;

                    int qty = store.ActualReceivedQuantity!.Value;
                    int whId = store.WarehouseId!.Value;
                    int variantId = detail.ProductVariantId;
                    string productName = detail.ProductVariant?.Product?.Name?.Trim() ?? "[Unknown]";

                    // Skip if already audited for same change
                    bool alreadyAudited = await _context.WareHouseStockAudits.AnyAsync(a =>
                        a.WareHouseStock.WareHouseId == whId &&
                        a.WareHouseStock.VariantId == variantId &&
                        a.QuantityChange == qty &&
                        a.Note.Contains(productName)
                    );
                    if (alreadyAudited)
                        continue;

                    // Find or create stock record
                    var stock = await _context.WareHousesStocks
                        .FirstOrDefaultAsync(s => s.WareHouseId == whId && s.VariantId == variantId);

                    string action;
                    if (stock == null)
                    {
                        stock = new WareHousesStock
                        {
                            WareHouseId = whId,
                            VariantId = variantId,
                            StockQuantity = qty
                        };
                        _context.WareHousesStocks.Add(stock);
                        action = "CREATE";
                    }
                    else
                    {
                        stock.StockQuantity += qty;
                        action = "INCREASE";
                    }

                    // Add audit log
                    _context.WareHouseStockAudits.Add(new WareHouseStockAudit
                    {
                        WareHouseStock = stock,
                        Action = action,
                        QuantityChange = qty,
                        ActionDate = DateTime.Now,
                        ChangedBy = accountId,
                        Note = $"Nhập kho thành công !"
                    });

                    // Mark store detail as processed
                    store.Status = "Success";
                    _context.ImportStoreDetails.Update(store);
                }

                // Save all changes and commit
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }



        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateWarehouseStockForSingleDetailAsync(
    ImportStoreDetail storeDetail,
    int productVariantId,
    int staffId)
        {
            // 1. Lấy accountId từ staffId
            var accountId = await _staffDetail.GetAccountIdByStaffIdAsync(staffId);
          

            // 2. Lấy số lượng và warehouseId
            int actualReceivedQuan = (int)storeDetail.ActualReceivedQuantity;
            int warehouseId = (int)storeDetail.WarehouseId;

            // 3. Tìm hoặc tạo bản ghi kho
            var wareHouseStock = await _context.WareHousesStocks
                .FirstOrDefaultAsync(ws =>
                    ws.WareHouseId == warehouseId &&
                    ws.VariantId == productVariantId);

            string auditAction;
            if (wareHouseStock == null)
            {
                wareHouseStock = new WareHousesStock
                {
                    WareHouseId = warehouseId,
                    VariantId = productVariantId,
                    StockQuantity = actualReceivedQuan
                };
                _context.WareHousesStocks.Add(wareHouseStock);
                await _context.SaveChangesAsync();  // để có WareHouseStockId
                auditAction = "CREATE";
            }
            else
            {
                wareHouseStock.StockQuantity += actualReceivedQuan;
                auditAction = "INCREASE";
            }

            // 4. Tạo audit, dùng accountId cho ChangedBy
            var stockAudit = new WareHouseStockAudit
            {
                WareHouseStockId = wareHouseStock.WareHouseStockId,
                Action = auditAction,
                QuantityChange = actualReceivedQuan,
                ActionDate = DateTime.Now,
                ChangedBy = accountId,
                Note = $"cập nhật vào kho thành công !"
            };
            _context.WareHouseStockAudits.Add(stockAudit);

            // 5. Lưu thay đổi
            await _context.SaveChangesAsync();
        }



        public async Task UpdateDispatchWarehouseStockAsync(
    Dispatch dispatch,
    int staffId,
    List<int> confirmedStoreDetailIds)
        {
            // 1. Bắt đầu transaction
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. Lấy accountId từ staffId
                var accountId = await _staffDetail.GetAccountIdByStaffIdAsync(staffId);
             

                // 3. Duyệt qua các DispatchDetail và chỉ xử lý những storeDetail vừa confirmed
                foreach (var dispatchDetail in dispatch.DispatchDetails)
                {
                    int variantId = dispatchDetail.VariantId;

                    foreach (var storeDetail in dispatchDetail.StoreExportStoreDetails
                                 .Where(sd => confirmedStoreDetailIds.Contains(sd.DispatchStoreDetailId)))
                    {
                        // 3.1. Tính số lượng thực đã dispatch
                        int actualDispatchedQty = storeDetail.ActualQuantity ?? storeDetail.AllocatedQuantity;
                        int warehouseId = storeDetail.WarehouseId;

                        // 3.2. Lấy hoặc tạo record kho
                        var wareHouseStock = await _context.WareHousesStocks
                            .FirstOrDefaultAsync(ws => ws.WareHouseId == warehouseId && ws.VariantId == variantId);

                        string action;
                        if (wareHouseStock == null)
                        {
                            wareHouseStock = new WareHousesStock
                            {
                                WareHouseId = warehouseId,
                                VariantId = variantId,
                                StockQuantity = -actualDispatchedQty
                            };
                            _context.WareHousesStocks.Add(wareHouseStock);
                            action = "CREATE";
                        }
                        else
                        {
                            wareHouseStock.StockQuantity -= actualDispatchedQty;
                            action = "DECREASE";
                        }

                        // 3.3. Tạo audit log, EF Core sẽ tự liên kết wareHouseStock
                        var stockAudit = new WareHouseStockAudit
                        {
                            WareHouseStock = wareHouseStock,
                            Action = action,
                            QuantityChange = -actualDispatchedQty,
                            ActionDate = DateTime.Now,
                            ChangedBy = accountId,
                            Note = "Đơn hàng đã được xuất !"
                        };
                        _context.WareHouseStockAudits.Add(stockAudit);
                    }
                }

                // 4. Lưu chung một lần và commit
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }


        public async Task<PaginatedResponseDTO<WarehouseStockDto>> GetAllWareHouse(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.WareHousesStocks
                .AsNoTracking()
                .Select(ws => new WarehouseStockDto
                {
                    WareHouseStockId = ws.WareHouseStockId,
                    VariantId = ws.VariantId,
                    StockQuantity = ws.StockQuantity,
                    WareHouseId = ws.WareHouseId,
                    WarehouseName = ws.WareHouse.WarehouseName,
                    FullProductName = ws.Variant.Product.Name
                                        + " " + ws.Variant.Color.ColorName
                                        + " " + ws.Variant.Size.SizeName
                });

            var total = await query.CountAsync(cancellationToken);
            var data = await query
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync(cancellationToken);

            return new PaginatedResponseDTO<WarehouseStockDto>(data, total, page, pageSize);
        }
        public async Task UpdateWarehouseStockForSingleDispatchDetailAsync(
    StoreExportStoreDetail storeDetail,
    int productVariantId,
    int staffId)
        {
            // 0. Bắt đầu transaction
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Lấy accountId từ staffId
                var accountId = await _staffDetail.GetAccountIdByStaffIdAsync(staffId);

                // 2. Tính số lượng thực tế đã dispatch
                int actualDispatchedQty = storeDetail.ActualQuantity ?? storeDetail.AllocatedQuantity;
                int warehouseId = storeDetail.WarehouseId;

                // 3. Lấy hoặc tạo stock record
                var wareHouseStock = await _context.WareHousesStocks
                    .FirstOrDefaultAsync(ws => ws.WareHouseId == warehouseId && ws.VariantId == productVariantId);

                string action;
                if (wareHouseStock == null)
                {
                    wareHouseStock = new WareHousesStock
                    {
                        WareHouseId = warehouseId,
                        VariantId = productVariantId,
                        StockQuantity = -actualDispatchedQty
                    };
                    _context.WareHousesStocks.Add(wareHouseStock);
                    action = "CREATE";
                }
                else
                {
                    wareHouseStock.StockQuantity -= actualDispatchedQty;
                    action = "DECREASE";
                }

                // 4. Tạo audit log, EF Core sẽ tự gán foreign key cho WareHouseStock
                var stockAudit = new WareHouseStockAudit
                {
                    WareHouseStock = wareHouseStock,
                    Action = action,
                    QuantityChange = -actualDispatchedQty,
                    ActionDate = DateTime.Now,
                    ChangedBy = accountId,
                    Note = "Đơn hàng đã được xuất !"
                };
                _context.WareHouseStockAudits.Add(stockAudit);

                // 5. Commit tất cả thay đổi
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

    }
}

