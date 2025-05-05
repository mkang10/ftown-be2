using Domain.DTO.Response;
using Domain.DTO.Response.Domain.DTO.Response;
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
        public async Task UpdateWarehouseStockAsync(Import import, int staffId)
        {
            // Chỉ xử lý các import có ImportType là Purchase
            if (import.ImportType?.Trim() != "Purchase")
                return;

            // Tạo các cặp (ImportDetail, ImportStoreDetail) cần xử lý, chỉ những storeDetail chưa được xử lý (Status != "Done")
            var detailStorePairs = import.ImportDetails
       .SelectMany(d => d.ImportStoreDetails
           .Where(sd => sd.ActualReceivedQuantity.HasValue
                        && sd.WarehouseId.HasValue
                        && sd.Status?.Trim() != "Success")
           .Select(sd => new { Detail = d, StoreDetail = sd }))
       .ToList();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var pair in detailStorePairs)
                {
                    var importDetail = pair.Detail;
                    var storeDetail = pair.StoreDetail;

                    int variantId = importDetail.ProductVariantId;
                    int receivedQty = storeDetail.ActualReceivedQuantity!.Value;
                    int warehouseId = storeDetail.WarehouseId!.Value;
                    string productName = importDetail.ProductVariant?.Product?.Name ?? "[Unknown Product]";

                    // Tìm WareHousesStock tương ứng
                    var warehouseStock = await _context.WareHousesStocks
                        .FirstOrDefaultAsync(ws => ws.WareHouseId == warehouseId && ws.VariantId == variantId);

                    string auditAction;
                    if (warehouseStock == null)
                    {
                        warehouseStock = new WareHousesStock
                        {
                            WareHouseId = warehouseId,
                            VariantId = variantId,
                            StockQuantity = receivedQty
                        };
                        _context.WareHousesStocks.Add(warehouseStock);
                        auditAction = "CREATE";
                    }
                    else
                    {
                        warehouseStock.StockQuantity += receivedQty;
                        auditAction = "INCREASE";
                    }

                    // Đánh dấu storeDetail đã xử lý
                    storeDetail.Status = "Done";
                    _context.ImportStoreDetails.Update(storeDetail);

                    // Tạo và thêm audit record
                    var auditRecord = new WareHouseStockAudit
                    {
                        WareHouseStock = warehouseStock,
                        Action = auditAction,
                        QuantityChange = receivedQty,
                        ActionDate = DateTime.Now,
                        ChangedBy = staffId,
                        Note = $"Nhập kho {receivedQty} x ‘{productName}’ thành công."
                    };
                    _context.WareHouseStockAudits.Add(auditRecord);
                }

                // Lưu tất cả thay đổi và commit transaction
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
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



        public async Task UpdateDispatchWarehouseStockAsync(Dispatch dispatch, int staffId)
        {


            // Duyệt qua từng DispatchDetail trong Dispatch
            foreach (var dispatchDetail in dispatch.DispatchDetails)
            {
                int variantId = dispatchDetail.VariantId;
                foreach (var storeDetail in dispatchDetail.StoreExportStoreDetails)
                {
                    // Giả sử: Nếu không có giá trị ActualDispatchedQuantity, ta dùng AllocatedQuantity
                    int actualDispatchedQuan = storeDetail.ActualQuantity.HasValue
                        ? storeDetail.ActualQuantity.Value
                        : storeDetail.AllocatedQuantity;

                    int warehouseId = storeDetail.WarehouseId; // WarehouseId là int, không cần nullable

                    // Tìm WareHousesStock theo WarehouseId và VariantId
                    var wareHouseStock = await _context.WareHousesStocks
                        .FirstOrDefaultAsync(ws => ws.WareHouseId == warehouseId && ws.VariantId == variantId);

                    string auditAction = "";
                    if (wareHouseStock == null)
                    {
                        // Nếu không có bản ghi: tạo mới với số lượng âm (giảm kho)
                        wareHouseStock = new WareHousesStock
                        {
                            WareHouseId = warehouseId,
                            VariantId = variantId,
                            StockQuantity = -actualDispatchedQuan
                        };
                        _context.WareHousesStocks.Add(wareHouseStock);
                        // Lưu ngay để nhận WareHouseStockId
                        await _context.SaveChangesAsync();
                        auditAction = "CREATE";
                    }
                    else
                    {
                        // Nếu đã có: giảm số lượng tồn kho
                        wareHouseStock.StockQuantity -= actualDispatchedQuan;
                        auditAction = "DECREASE";
                    }

                    // Tạo WareHouseStockAudit cho giao dịch cập nhật tồn kho
                    var stockAudit = new WareHouseStockAudit
                    {
                        WareHouseStockId = wareHouseStock.WareHouseStockId, // Đã được gán sau SaveChanges nếu là bản ghi mới
                        Action = auditAction,
                        // Ghi nhận sự thay đổi dưới dạng giá trị âm để thể hiện việc giảm kho
                        QuantityChange = -actualDispatchedQuan,
                        ActionDate = DateTime.Now,
                        ChangedBy = staffId,
                        Note = $"Updated via Dispatch Done. DispatchDetailId: {dispatchDetail.DispatchDetailId}, DispatchStoreDetailId: {storeDetail.DispatchStoreDetailId}"
                    };
                    _context.WareHouseStockAudits.Add(stockAudit);
                }
            }

            // Cuối cùng lưu lại các bản ghi audit và cập nhật kho
            await _context.SaveChangesAsync();
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
        public async Task UpdateWarehouseStockForSingleDispatchDetailAsync(StoreExportStoreDetail storeDetail, int productVariantId, int staffId)
        {
            // Lấy số lượng thực tế đã dispatch (nếu có, nếu không lấy AllocatedQuantity)
            int actualDispatchedQuan = storeDetail.ActualQuantity.HasValue
                ? storeDetail.ActualQuantity.Value
                : storeDetail.AllocatedQuantity;

            int warehouseId = storeDetail.WarehouseId; // Giả sử WarehouseId là int (không nullable)

            // Tìm WareHousesStock theo WarehouseId và VariantId
            var wareHouseStock = await _context.WareHousesStocks
                .FirstOrDefaultAsync(ws => ws.WareHouseId == warehouseId && ws.VariantId == productVariantId);

            string auditAction = "";
            if (wareHouseStock == null)
            {
                // Nếu chưa có bản ghi kho, tạo mới với StockQuantity âm để biểu thị việc giảm số lượng
                wareHouseStock = new WareHousesStock
                {
                    WareHouseId = warehouseId,
                    VariantId = productVariantId,
                    StockQuantity = -actualDispatchedQuan
                };
                _context.WareHousesStocks.Add(wareHouseStock);
                // Lưu ngay để nhận WareHouseStockId
                await _context.SaveChangesAsync();
                auditAction = "CREATE";
            }
            else
            {
                // Nếu đã có: giảm số lượng tồn kho
                wareHouseStock.StockQuantity -= actualDispatchedQuan;
                auditAction = "DECREASE";
            }

            // Tạo WareHouseStockAudit cho giao dịch cập nhật kho (ghi nhận số lượng thay đổi dưới dạng số âm)
            var stockAudit = new WareHouseStockAudit
            {
                WareHouseStockId = wareHouseStock.WareHouseStockId, // Đã được gán sau SaveChanges nếu là bản ghi mới
                Action = auditAction,
                QuantityChange = -actualDispatchedQuan, // Số lượng giảm nên giá trị âm
                ActionDate = DateTime.Now,
                ChangedBy = staffId,
                Note = $"Updated via Dispatch Done. DispatchStoreDetailId: {storeDetail.DispatchStoreDetailId}"
            };
            _context.WareHouseStockAudits.Add(stockAudit);

            // Lưu lại các thay đổi cho WareHousesStock và WareHouseStockAudit
            await _context.SaveChangesAsync();
        }
    }
}

