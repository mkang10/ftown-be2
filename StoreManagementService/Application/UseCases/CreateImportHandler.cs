using Application.DTO.Request;
using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.DTO.Response.Application.Imports.Dto;
using Domain.DTO.Response.Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class CreateImportHandler
    {
        private readonly Random _rng = new();

        private readonly IWarehouseRepository _warehouseRepo;
        private readonly IWareHouseStockRepos _stockRepo;
        private readonly IProductVariantRepository _productVariantRepo;
        private readonly ITransferRepository _transferRepo;
        private readonly ITransferDetailRepository _transferDetailRepo;
        private readonly IImportRepos _importRepo;
        private readonly IAuditLogRepos _auditLogRepo;
        private readonly IDispatchRepos _dispatchRepos;
        private readonly IImportStoreRepos _importStoreRepo;
        private readonly IWarehouseStaffRepos _wsRepos;
        private readonly IDispatchDetailRepository _dispatchDetail;
        private readonly IStoreExportRepos _storeExportRepos;
        public CreateImportHandler(
            IWarehouseRepository warehouseRepo,
            IWareHouseStockRepos stockRepo,
            IProductVariantRepository productVariantRepo,
            ITransferRepository transferRepo,
            ITransferDetailRepository transferDetailRepo,
            IImportRepos importRepo,
            IAuditLogRepos auditLogRepo,
            IDispatchRepos dispatchRepos,
            IImportStoreRepos importStoreRepo,
            IWarehouseStaffRepos wsRepos,
            IDispatchDetailRepository dispatchDetail,
            IStoreExportRepos storeExportRepos)
        {
            _warehouseRepo = warehouseRepo;
            _stockRepo = stockRepo;
            _productVariantRepo = productVariantRepo;
            _transferRepo = transferRepo;
            _transferDetailRepo = transferDetailRepo;
            _importRepo = importRepo;
            _auditLogRepo = auditLogRepo;
            _dispatchRepos = dispatchRepos;
            _importStoreRepo = importStoreRepo;
            _wsRepos = wsRepos;
            _dispatchDetail = dispatchDetail;
            _storeExportRepos = storeExportRepos;
        }

        public async Task<ResponseDTO<ImportResponseDto>> CreateTransferImportAsync(TransImportDto dto)
        {
            try
            {
                // 1. Validate
                // 1. Validate DTO and stock levels
                var validation = ValidateDto(dto) ?? await ValidateStockLimitsAsync(dto);
                if (validation != null)
                    return validation;

                // 2. Create and save Import
                var importEntity = BuildImportEntity(dto);

                // 3. Auto-approve/reject based on business rules
                await EvaluateAutoApprovalAsync(importEntity, dto);

               
                await SaveImportAsync(importEntity);
              

                // 3. Audit log for Import
                await CreateAuditLogAsync(
                    table: "Import",
                    recordId: importEntity.ImportId.ToString(),
                    changedBy: importEntity.CreatedBy,
                    comment: "Tạo yêu cầu nhập hàng"
                );

                // 4. Load Warehouse
                var warehouse = await _warehouseRepo.GetByIdAsync(dto.WarehouseId);
                if (warehouse == null)
                    return new ResponseDTO<ImportResponseDto>(null, false, "Không tìm thấy warehouse.");

                var allWarehouses = await _warehouseRepo.GetAllAsync();

                // 2. Tách warehouses khác để dùng cho export
                var otherWarehouseIds = allWarehouses
                    .Where(w => w.WarehouseId != dto.WarehouseId)
                    .Select(w => w.WarehouseId)
                    .ToList();


                // 5. Allocate Checkers
                var allCheckers = await _wsRepos.GetByWarehouseAndRoleAsync(warehouse.WarehouseId, "Checker");
                var unused = GetInitialUnusedCheckers(warehouse.UnusedCheckerIds, allCheckers);

                var exportCheckers = new List<WarehouseStaff>();
                foreach (var wid in otherWarehouseIds)
                {
                    var checkersInOther = await _wsRepos.GetByWarehouseAndRoleAsync(wid, "Checker");
                    exportCheckers.AddRange(checkersInOther);
                }
                var exportUnused = GetInitialUnusedCheckers(warehouse.UnusedCheckerIds, exportCheckers);

                // 6. Create and save ImportStoreDetails
                var storeDetails = BuildStoreDetails(importEntity, dto.WarehouseId, (int)warehouse.ShopManagerId, allCheckers, unused);
                await _importStoreRepo.AddRangeAsync(storeDetails);
                await _importRepo.SaveChangesAsync();

                // 7. Audit log for each StoreDetail
                foreach (var sd in storeDetails)
                {
                    await CreateAuditLogAsync(
                        table: "ImportStoreDetail",
                        recordId: sd.ImportStoreId.ToString(),
                        changedBy: importEntity.CreatedBy,
                        comment: $"Tạo mới ImportStoreDetail cho ImportId {importEntity.ImportId}"
                    );
                }

                // 8. Update Warehouse unused list
                warehouse.UnusedCheckerIds = string.Join(",", unused);
                await _warehouseRepo.UpdateAsync(warehouse);
                //Nếu bị từ chối thì trả về luôn với message cụ thể
                // === BỔ SUNG: Nếu import bị REJECTED thì cập nhật các ImportStoreDetail thành "Rejected" ===
                if (string.Equals(importEntity.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var sd in storeDetails)
                    {
                        // Chuyển status
                        sd.Status = "Rejected";
                        await _importStoreRepo.UpdateAsync(sd);

                        // Audit log
                        await CreateAuditLogAsync(
                            table: "ImportStoreDetail",
                            recordId: sd.ImportStoreId.ToString(),
                            changedBy: importEntity.CreatedBy,
                            comment: "Cập nhật trạng thái từ Processing → Rejected do Import bị từ chối"
                        );
                    }
                    // Lưu các thay đổi
                    await _importRepo.SaveChangesAsync();

                    // Trả về luôn (Status = true), client sẽ biết import đã xử lý xong nhưng status là Rejected
                    var rejectedDto = new ImportResponseDto { ImportId = importEntity.ImportId };
                    return new ResponseDTO<ImportResponseDto>(
                        rejectedDto,
                        true,
                        "Đơn nhập đã bị từ chối, các đơn con đã được chuyển sang trạng thái Rejected."
                    );
                }


                // === BƯỚC 9: Nếu tự phê duyệt thành công thì sinh Dispatch → Transfer → Details ===
                if (importEntity.Status == "Approved")
                {
                    // 9.1 Tạo Dispatch trước
                    var dispatch = new Dispatch
                    {
                        CreatedBy = importEntity.CreatedBy,
                        CreatedDate = DateTime.UtcNow,
                        Status = "Approved",
                        ReferenceNumber = $"DP-{DateTime.UtcNow:yyyyMMddHHmmss}",
                        Remarks = "Dispatch tự động cho Transfer",
                        OriginalId = importEntity.ImportId  // hoặc transferOrderId nếu bạn muốn
                    };
                    await _dispatchRepos.AddAsync(dispatch);
                    await _dispatchRepos.SaveChangesAsync();

                    await CreateAuditLogAsync(
                        table: "Dispatch",
                        recordId: dispatch.DispatchId.ToString(),
                        changedBy: dispatch.CreatedBy,
                        comment: "Tạo Dispatch tự động sau phê duyệt Import"
                    );

                    // 9.2 Tạo Transfer, gán DispatchId từ dispatch vừa tạo
                    var transfer = new Transfer
                    {
                        ImportId = importEntity.ImportId,
                        DispatchId = dispatch.DispatchId,
                        CreatedBy = importEntity.CreatedBy,
                        CreatedDate = DateTime.UtcNow,
                        Status = "Approved",
                        Remarks = "Tự động sinh khi Import được phê duyệt"
                    };
                    await _transferRepo.AddAsync(transfer);

                    await CreateAuditLogAsync(
                        table: "Transfer",
                        recordId: transfer.TransferOrderId.ToString(),
                        changedBy: transfer.CreatedBy,
                        comment: "Tạo Transfer tự động liên kết Dispatch"
                    );

                    // 9.3 Tạo TransferDetail
                    var transferDetails = importEntity.ImportDetails
                        .SelectMany(impD =>
                        {
                            var allocations = storeDetails
                                .Where(sd => sd.ImportDetailId == impD.ImportDetailId)
                                .ToList();

                            if (allocations.Any())
                            {
                                return allocations.Select(sd => new TransferDetail
                                {
                                    TransferOrderId = transfer.TransferOrderId,
                                    VariantId = impD.ProductVariantId,
                                    Quantity = sd.AllocatedQuantity
                                });
                            }
                            else
                            {
                                return new[]
                                {
                    new TransferDetail
                    {
                        TransferOrderId = transfer.TransferOrderId,
                        VariantId       = impD.ProductVariantId,
                        Quantity        = impD.Quantity
                    }
                                };
                            }
                        })
                        .ToList();

                    await _transferDetailRepo.AddRangeAndSaveAsync(transferDetails);

                    foreach (var td in transferDetails)
                    {
                        await CreateAuditLogAsync(
                            table: "TransferDetail",
                            recordId: td.TransferOrderDetailId.ToString(),
                            changedBy: transfer.CreatedBy,
                            comment: $"Tạo TransferDetail (Variant {td.VariantId})"
                        );
                    }

                    // 9.4 Tạo DispatchDetail
                    var dispatchDetails = transferDetails.Select(td => new DispatchDetail
                    {
                        DispatchId = dispatch.DispatchId,
                        VariantId = td.VariantId,
                        Quantity = td.Quantity
                    }).ToList();

                    await _dispatchDetail.AddRangeAndSaveAsync(dispatchDetails);
                    await _dispatchRepos.SaveChangesAsync();

                    foreach (var dd in dispatchDetails)
                    {
                        await CreateAuditLogAsync(
                            table: "DispatchDetail",
                            recordId: dd.DispatchDetailId.ToString(),
                            changedBy: dispatch.CreatedBy,
                            comment: $"Tạo DispatchDetail (Variant {dd.VariantId})"
                        );
                    }

                    // 9.5 Tạo StoreExportStoreDetail
                    var exportDetails = BuildExportStoreDetails(
     dispatchDetails,
     dto.WarehouseId,
     (int)warehouse.ShopManagerId,
     exportCheckers,
     exportUnused
 );

                    await _storeExportRepos.AddRangeAndSaveAsync(exportDetails);
                    foreach (var ed in exportDetails)
                    {
                        await CreateAuditLogAsync(
                            table: "StoreExportStoreDetail",
                            recordId: ed.DispatchStoreDetailId.ToString(),
                            changedBy: dispatch.CreatedBy,
                            comment: $"Tạo StoreExportStoreDetail cho DispatchDetail {ed.DispatchDetailId}"
                        );
                    }
                }

                var resultDto = new ImportResponseDto
                {
                    ImportId = importEntity.ImportId,
                };


                return new ResponseDTO<ImportResponseDto>(resultDto, true,
            "Tạo đơn nhập hàng thành công.");
            }
            catch (Exception ex)
            {
                return new ResponseDTO<ImportResponseDto>(null, false, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }


        private async Task<Dictionary<int, string>> EvaluateAutoApprovalAsync(Import importEntity, TransImportDto dto)
        {
            var detailComments = new Dictionary<int, string>();
            bool allCapable = true;

            // Lấy flag isUrgent trực tiếp từ DTO (nullable bool)

            foreach (var detail in dto.ImportDetails)
            {
                // Truyền flag isUrgent lấy từ request
                var stores = await GetStoresByAvailableStockAsync(
                    detail.ProductVariantId,
                    dto.WarehouseId,
                    dto.IsUrgent);
                var maxAvailable = stores.Any() ? stores.Max(s => s.available) : 0;

                if (maxAvailable < detail.Quantity)
                {
                    allCapable = false;
                    detailComments[detail.ProductVariantId] =
                        $"Không đủ khả năng nhập (cần: {detail.Quantity}, tối đa: {maxAvailable})";
                }
                else
                {
                    detailComments[detail.ProductVariantId] =
                        "Đơn Nhập Hàng Tự Động bởi hệ thống";
                }
            }

            importEntity.Status = allCapable ? "Approved" : "Rejected";
            return detailComments;
        }

        // Nếu tất cả chi tiết đều có thể nhập tại ít nhất 1 kho con

        private List<StoreExportStoreDetail> BuildExportStoreDetails(
    IEnumerable<DispatchDetail> dispatchDetails,
    int warehouseId,
    int shopManagerId,
    IEnumerable<WarehouseStaff> allCheckers,
    List<int> unused)
        {
            var details = new List<StoreExportStoreDetail>();

            foreach (var dd in dispatchDetails)
            {
                // Nếu đã dùng hết checker thì reset lại từ đầu
                if (!unused.Any())
                    unused = allCheckers.Select(w => w.StaffDetailId).ToList();

                int idx = _rng.Next(unused.Count);
                int chosen = unused[idx];
                unused.RemoveAt(idx);

                details.Add(new StoreExportStoreDetail
                {
                    DispatchDetailId = dd.DispatchDetailId,
                    WarehouseId = warehouseId,
                    AllocatedQuantity = dd.Quantity,
                    Status = "Processing",
                    Comments = "Đơn xuất hàng được sinh tự động",
                    StaffDetailId = chosen,
                    HandleBy = shopManagerId
                });
            }

            return details;
        }

        private async Task<List<(int warehouseId, int available)>> GetStoresByAvailableStockAsync(
    int variantId,
    int excludeWarehouseId,
    bool isUrgent)
        {
            // Lấy tất cả kho, loại bỏ kho đang import
            var allWarehouses = (await _warehouseRepo.GetAllAsync())
                .Where(w => w.WarehouseId != excludeWarehouseId)
                .ToList();

            // Ưu tiên kho con (IsOwnerWarehouse = false) trước, rồi kho tổng
            var storesOrdered = allWarehouses
                .OrderBy(w => w.IsOwnerWarehouse)
                .ToList();

            var result = new List<(int warehouseId, int available)>();

            foreach (var wh in storesOrdered)
            {
                var stock = await _stockRepo.GetByWarehouseAndVariantAsync(wh.WarehouseId, variantId);
                var qty = stock?.StockQuantity ?? 0;

                // Chọn mức safetyStock phù hợp
                var safetyThreshold = isUrgent
                    ? (wh.UrgentSafetyStock ?? wh.SafetyStock)
                    : wh.SafetyStock;

                var pendingOutbound = await _dispatchRepos.GetApprovedOutboundQuantityAsync(
                    wh.WarehouseId,
                    variantId
                );

                var available = qty - safetyThreshold - pendingOutbound;
                result.Add((wh.WarehouseId, (int)available));
            }

            return result;
        }

        private ResponseDTO<ImportResponseDto> ValidateDto(TransImportDto dto)
        {
            if (dto.ImportDetails == null || !dto.ImportDetails.Any())
                return new ResponseDTO<ImportResponseDto>(null, false, "Phải có ít nhất 1 sản phẩm.");
            return null;
        }

        // 2. Hàm ValidateStockLimitsAsync (chú ý dùng GetByIdAsync, không phải GetByIdsAsync)
        private async Task<ResponseDTO<ImportResponseDto>> ValidateStockLimitsAsync(TransImportDto dto)
        {
            foreach (var detail in dto.ImportDetails)
            {
                // Lấy stock hiện tại của variant
                var stock = await _stockRepo.GetByWarehouseAndVariantAsync(dto.WarehouseId, detail.ProductVariantId);
                var currentQty = stock?.StockQuantity ?? 0;

                // Lấy thông tin variant (dùng GetByIdAsync)
                var variant = await _productVariantRepo.GetByIdAsync(detail.ProductVariantId);
                if (variant == null)
                    return new ResponseDTO<ImportResponseDto>(null, false,
                        $"Không tìm thấy sản phẩm variant {detail.ProductVariantId}.");

                // Kiểm tra giới hạn tồn kho
                if (currentQty + detail.Quantity > variant.MaxStocks)
                {
                    var possibleQty = variant.MaxStocks - currentQty;
                    return new ResponseDTO<ImportResponseDto>(null, false,
                        $"Sản phẩm (ID {variant.VariantId}) hiện tại có {currentQty} trong kho, " +
                        $"yêu cầu thêm {detail.Quantity}, chỉ có thể nhập tối đa {possibleQty} nữa (tổng {variant.MaxStocks}).");
                }
            }

            return null;
        }

        public async Task<ResponseDTO<ImportResponseDto>> CreateTRansferImportFromExcelAsync(IFormFile file, int warehouseId, int createdBy)
        {
            if (file == null || file.Length == 0)
                return new ResponseDTO<ImportResponseDto>(null, false, "Vui lòng chọn file Excel.");

            var details = new List<TransImportDetailDto>();
            using (var stream = file.OpenReadStream())
            using (var workbook = new XLWorkbook(stream))
            {
                var sheet = workbook.Worksheet(1);
                var firstRow = sheet.FirstRowUsed();
                if (firstRow == null)
                    return new ResponseDTO<ImportResponseDto>(null, false, "File Excel không có dữ liệu.");

                int row = firstRow.RowNumber() + 1;
                while (true)
                {
                    var skuCell = sheet.Cell(row, 1);
                    if (skuCell.IsEmpty()) break;

                    string sku = skuCell.GetString().Trim();
                    var variant = await _productVariantRepo.GetBySkuAsync(sku);
                    if (variant == null)
                        return new ResponseDTO<ImportResponseDto>(null, false, $"Dòng {row}: Không tìm thấy variant với SKU '{sku}'.");

                    int qty = sheet.Cell(row, 2).GetValue<int>();

                    details.Add(new TransImportDetailDto
                    {
                        ProductVariantId = variant.VariantId,
                        Quantity = qty,
                    });

                    row++;
                }
            }

            if (!details.Any())
                return new ResponseDTO<ImportResponseDto>(null, false, "File Excel không có dòng dữ liệu hợp lệ.");

            var dto = new TransImportDto
            {
                CreatedBy = createdBy,
                WarehouseId = warehouseId,
                ImportDetails = details,
                IsUrgent = false
                
                
            };

            return await CreateTransferImportAsync(dto);
        }




        private Import BuildImportEntity(TransImportDto dto)
        {
            var importEntity = new Import
            {
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTime.Now,
                Status = "Pending",
                ImportType = "Transfer",
                ReferenceNumber = GenerateReferenceNumber(),
                IsUrgent = (bool)dto.IsUrgent,
                ImportDetails = dto.ImportDetails.Select(d => new ImportDetail
                {
                    ProductVariantId = d.ProductVariantId,
                    Quantity = d.Quantity,
                    CostPrice = 0,
                }).ToList()
            };
            importEntity.TotalCost = 0;
            return importEntity;
        }

        private async Task SaveImportAsync(Import importEntity)
        {
            await _importRepo.AddAsync(importEntity);
            await _importRepo.SaveChangesAsync();
        }

        private string GenerateReferenceNumber()
            => $"IMP-TRS-{DateTime.Now:yyyyMMddHHmmss}";

        private List<int> GetInitialUnusedCheckers(string storedUnused, IEnumerable<WarehouseStaff> all)
        {
            var unused = string.IsNullOrWhiteSpace(storedUnused)
                ? all.Select(w => w.StaffDetailId).ToList()
                : storedUnused.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

            if (!unused.Any())
                unused = all.Select(w => w.StaffDetailId).ToList();

            return unused;
        }

        private List<ImportStoreDetail> BuildStoreDetails(
            Import importEntity,
            int warehouseId,
            int shopManagerId,
            IEnumerable<WarehouseStaff> allCheckers,
            List<int> unused)
        {
            var details = new List<ImportStoreDetail>();

            foreach (var det in importEntity.ImportDetails)
            {
                if (!unused.Any())
                    unused = allCheckers.Select(w => w.StaffDetailId).ToList();

                int idx = _rng.Next(unused.Count);
                int chosen = unused[idx];
                unused.RemoveAt(idx);

                details.Add(new ImportStoreDetail
                {
                    ImportDetailId = det.ImportDetailId,
                    WarehouseId = warehouseId,
                    AllocatedQuantity = det.Quantity,
                    Status = "Processing",
                    Comments = "Đơn Nhập Hàng Tự Động bởi hệ thống",
                    StaffDetailId = chosen,
                    HandleBy = shopManagerId
                });
            }

            return details;
        }

        private async Task CreateAuditLogAsync(string table, string recordId, int changedBy, string comment)
        {
            var log = new AuditLog
            {
                TableName = table,
                RecordId = recordId,
                Operation = "CREATE",
                ChangeDate = DateTime.Now,
                ChangedBy = changedBy,
                ChangeData = string.Empty,
                Comment = comment
            };
            await _auditLogRepo.AddAsync(log);
            await _auditLogRepo.SaveChangesAsync();
        }
    }
}
