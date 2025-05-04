using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using System;
using Domain.Interfaces;
using Domain.DTO.Response;
using Domain.DTOs;

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
                // Include Variant và các con của nó
                .Include(ws => ws.Variant)
                    .ThenInclude(v => v.Product)
                .Include(ws => ws.Variant)
                    .ThenInclude(v => v.Color)
                .Include(ws => ws.Variant)
                    .ThenInclude(v => v.Size)
                // Include thông tin kho
                .Include(ws => ws.WareHouse)
                // Include audit history và user tạo thay đổi
                .Include(ws => ws.WareHouseStockAudits)
                    .ThenInclude(a => a.ChangedByNavigation)
                // Nếu bạn không sửa đổi entity thì tốt nhất dùng AsNoTracking()
                .AsNoTracking()
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
            // Chỉ xử lý các import có ImportType là Purchase (dùng Trim để bỏ khoảng trắng thừa)
            if (import.ImportType?.Trim() != "Purchase")
            {
                // Nếu không phải Purchase, không thực hiện cập nhật tồn kho
                return;
            }
            // Duyệt qua từng ImportDetail trong Import
            foreach (var importDetail in import.ImportDetails)
            {
                int variantId = importDetail.ProductVariantId;
                foreach (var storeDetail in importDetail.ImportStoreDetails)
                {
                    if (!storeDetail.ActualReceivedQuantity.HasValue || !storeDetail.WarehouseId.HasValue)
                    {
                        // Xử lý trường hợp giá trị null, ví dụ log lỗi, gán giá trị mặc định, hoặc bỏ qua cập nhật.
                        continue; // hoặc throw exception với thông báo rõ ràng
                    }
                    int actualReceivedQuan = storeDetail.ActualReceivedQuantity.Value;
                    int warehouseId = storeDetail.WarehouseId.Value;

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
                            StockQuantity = actualReceivedQuan
                        };
                        _context.WareHousesStocks.Add(wareHouseStock);
                        // Lưu ngay để nhận WareHouseStockId
                        await _context.SaveChangesAsync();
                        auditAction = "CREATE";
                    }
                    else
                    {
                        // Đã có: tăng số lượng tồn kho
                        wareHouseStock.StockQuantity += actualReceivedQuan;
                        auditAction = "INCREASE";
                    }

                    // Tạo WareHouseStockAudit cho giao dịch cập nhật tồn kho
                    var stockAudit = new WareHouseStockAudit
                    {
                        WareHouseStockId = wareHouseStock.WareHouseStockId, // Đã được gán sau SaveChanges nếu là bản ghi mới
                        Action = auditAction,
                        QuantityChange = actualReceivedQuan,
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

        public async Task UpdateWarehouseStockForSingleDetailAsync(ImportStoreDetail storeDetail, int productVariantId, int staffId)
        {
            // Lấy số lượng thực tế nhận được và warehouseId từ storeDetail
            int actualReceivedQuan = (int)storeDetail.ActualReceivedQuantity;
            int warehouseId = (int)storeDetail.WarehouseId;

            // Tìm WareHousesStock theo WarehouseId và VariantId
            var wareHouseStock = await _context.WareHousesStocks
                .FirstOrDefaultAsync(ws => ws.WareHouseId == warehouseId && ws.VariantId == productVariantId);

            string auditAction = "";
            if (wareHouseStock == null)
            {
                // Nếu chưa có bản ghi kho: tạo mới
                wareHouseStock = new WareHousesStock
                {
                    WareHouseId = warehouseId,
                    VariantId = productVariantId,
                    StockQuantity = actualReceivedQuan
                };
                _context.WareHousesStocks.Add(wareHouseStock);
                // Lưu ngay để nhận WareHouseStockId
                await _context.SaveChangesAsync();
                auditAction = "CREATE";
            }
            else
            {
                // Nếu đã có: tăng số lượng tồn kho
                wareHouseStock.StockQuantity += actualReceivedQuan;
                auditAction = "INCREASE";
            }

            // Tạo WareHouseStockAudit cho giao dịch cập nhật kho
            var stockAudit = new WareHouseStockAudit
            {
                WareHouseStockId = wareHouseStock.WareHouseStockId, // Đã có sau SaveChanges nếu là bản ghi mới
                Action = auditAction,
                QuantityChange = actualReceivedQuan,
                ActionDate = DateTime.Now,
                ChangedBy = staffId,
                Note = $"Updated via Import Done. ImportStoreId: {storeDetail.ImportStoreId}"
            };
            _context.WareHouseStockAudits.Add(stockAudit);

            // Lưu lại các thay đổi cho WareHouseStock và WareHouseStockAudit
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

     

