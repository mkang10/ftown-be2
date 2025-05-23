﻿using Domain.DTO.Response;
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

