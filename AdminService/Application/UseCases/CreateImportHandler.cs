using Application.DTO.Response;
using Application.Services;
using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Domain.DTO.Request;
using Domain.DTO.Response;
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

        private readonly IImportRepos _impRepos;
        private readonly IProductVarRepos _productVar;

        private readonly IImportStoreRepos _importStoreRepos;
        private readonly IWarehouseStaffRepos _wsRepos;
        private readonly IMapper _mapper;
        private readonly IAuditLogRepos _auditLogRepos;
        private readonly ReportService _reportService;


        public CreateImportHandler(ReportService reportService, IProductVarRepos productVar, IWarehouseRepository warehouseRepo, IWarehouseStaffRepos wsRepos, IImportRepos impRepos, IImportStoreRepos importStoreRepos, IMapper mapper, IAuditLogRepos auditLogRepos)
        {
            _importStoreRepos = importStoreRepos;
            _productVar = productVar;
            _impRepos = impRepos;
            _mapper = mapper;
            _auditLogRepos = auditLogRepos;
            _reportService = reportService;
            _wsRepos = wsRepos;
            _warehouseRepo = warehouseRepo;
        }
        public async Task<ResponseDTO<Import>> CreatePurchaseImportAsync(PurchaseImportCreateDto dto)
        {
            try
            {
                // 1. Validate DTO
                if (dto.ImportDetails == null || !dto.ImportDetails.Any())
                    return new ResponseDTO<Import>(null, false, "Phải có ít nhất 1 sản phẩm.");
                if (dto.ImportDetails.Any(d => d.CostPrice <= 0))
                    return new ResponseDTO<Import>(null, false, "CostPrice phải lớn hơn 0.");

                // 2. Tạo Import + ImportDetails
                var importEntity = new Import
                {
                    CreatedBy = dto.CreatedBy,
                    ApprovedDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Status = "Approved",
                    ImportType = "Purchase",
                    ReferenceNumber = GenerateReferenceNumber(),
                    ImportDetails = dto.ImportDetails.Select(d => new ImportDetail
                    {
                        ProductVariantId = d.ProductVariantId,
                        Quantity = d.Quantity,
                        CostPrice = d.CostPrice
                    }).ToList()
                };
                importEntity.TotalCost = importEntity.ImportDetails.Sum(x => x.Quantity * x.CostPrice);

                // 3. Lưu Import vào DB
                await _impRepos.AddAsync(importEntity);
                await _impRepos.SaveChangesAsync();

                // 4. Ghi AuditLog cho Import vừa tạo
                var serializedImport = JsonConvert.SerializeObject(importEntity,
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                var auditLog = new AuditLog
                {
                    TableName = "Import",
                    RecordId = importEntity.ImportId.ToString(),
                    Operation = "CREATE",
                    ChangeDate = DateTime.Now,
                    ChangedBy = importEntity.CreatedBy,
                    ChangeData = serializedImport,
                    Comment = "Tạo mới đơn import Purchase"
                };
                await _auditLogRepos.AddAsync(auditLog);
                await _auditLogRepos.SaveChangesAsync();

                // 5. Lấy kho tổng của owner
                var warehouse = await _warehouseRepo.GetOwnerWarehouseAsync();
                if (warehouse == null)
                    return new ResponseDTO<Import>(null, false, "Không tìm thấy kho tổng của owner.");

                // 6. Lấy danh sách Checker
                var allCheckers = await _wsRepos.GetByWarehouseAndRoleAsync(warehouse.WarehouseId, "Checker");
                var unused = string.IsNullOrWhiteSpace(warehouse.UnusedCheckerIds)
                    ? allCheckers.Select(w => w.StaffDetailId).ToList()
                    : warehouse.UnusedCheckerIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();
                if (!unused.Any())
                    unused = allCheckers.Select(w => w.StaffDetailId).ToList();

                var storeDetails = new List<ImportStoreDetail>();
                foreach (var det in importEntity.ImportDetails)
                {
                    if (!unused.Any())
                        unused = allCheckers.Select(w => w.StaffDetailId).ToList();

                    int idx = _rng.Next(unused.Count);
                    int chosen = unused[idx];
                    unused.RemoveAt(idx);

                    storeDetails.Add(new ImportStoreDetail
                    {
                        ImportDetailId = det.ImportDetailId,
                        WarehouseId = warehouse.WarehouseId,
                        AllocatedQuantity = det.Quantity,
                        Status = "Processing",
                        Comments = "Đơn Nhập Hàng Tự Động bởi hệ thống",
                        StaffDetailId = chosen,
                        HandleBy = null
                    });
                }

                // 8. Lưu ImportStoreDetails
                await _importStoreRepos.AddRangeAsync(storeDetails);
                await _impRepos.SaveChangesAsync();  // hoặc SaveChanges của importStoreRepos

                // 9. Ghi AuditLog cho từng ImportStoreDetail
                foreach (var sd in storeDetails)
                {
                    var serializedSd = JsonConvert.SerializeObject(sd,
                        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    var auditLogSd = new AuditLog
                    {
                        TableName = "ImportStoreDetail",
                        RecordId = sd.ImportStoreId.ToString(),   // EF đã set ra ID sau SaveChanges
                        Operation = "CREATE",
                        ChangeDate = DateTime.Now,
                        ChangedBy = importEntity.CreatedBy,
                        ChangeData = serializedSd,
                        Comment = "Tạo mới ImportStoreDetail cho ImportId " + importEntity.ImportId
                    };
                    await _auditLogRepos.AddAsync(auditLogSd);
                }
                await _auditLogRepos.SaveChangesAsync();

                // 10. Cập nhật lại danh sách unused vào Warehouse
                warehouse.UnusedCheckerIds = string.Join(",", unused);
                await _warehouseRepo.UpdateAsync(warehouse);

                return new ResponseDTO<Import>(importEntity, true,
                    "Tạo đơn Purchase thành công, gán Checker và ghi AuditLog cho Import + StoreDetails.");
            }
            catch (Exception ex)
            {
                return new ResponseDTO<Import>(null, false, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }
        private string GenerateReferenceNumber()
            => $"IMP-PUR-{DateTime.Now:yyyyMMddHHmmss}";

        public async Task<ResponseDTO<Import>> CreatePurchaseImportFromExcelAsync(IFormFile file, int createdBy)
        {
            if (file == null || file.Length == 0)
                return new ResponseDTO<Import>(null, false, "Vui lòng chọn file Excel.");

            var details = new List<PurchaseImportDetailDto>();
            using (var stream = file.OpenReadStream())
            using (var workbook = new XLWorkbook(stream))
            {
                var sheet = workbook.Worksheet(1);
                var firstRow = sheet.FirstRowUsed();
                if (firstRow == null)
                    return new ResponseDTO<Import>(null, false, "File Excel không có dữ liệu.");

                int row = firstRow.RowNumber() + 1;
                while (true)
                {
                    var skuCell = sheet.Cell(row, 1);
                    if (skuCell.IsEmpty()) break;

                    string sku = skuCell.GetString().Trim();
                    var variant = await _productVar.GetBySkuAsync(sku);
                    if (variant == null)
                        return new ResponseDTO<Import>(null, false, $"Dòng {row}: Không tìm thấy variant với SKU '{sku}'.");

                    int qty = sheet.Cell(row, 2).GetValue<int>();
                    decimal cost = sheet.Cell(row, 3).GetValue<decimal>();

                    if (qty <= 0 || cost <= 0)
                        return new ResponseDTO<Import>(null, false, $"Dòng {row}: Quantity và CostPrice phải > 0.");

                    details.Add(new PurchaseImportDetailDto
                    {
                        ProductVariantId = variant.VariantId,
                        Quantity = qty,
                        CostPrice = cost
                    });

                    row++;
                }
            }

            if (!details.Any())
                return new ResponseDTO<Import>(null, false, "File Excel không có dòng dữ liệu hợp lệ.");

            var dto = new PurchaseImportCreateDto
            {
                CreatedBy = createdBy,
                ImportDetails = details
            };

            return await CreatePurchaseImportAsync(dto);
        }

        public async Task<ResponseDTO<ImportSupplementReportDto>> CreateSupplementImportAsync(SupplementImportRequestDto request)
        {
            try
            {
                // Kiểm tra đầu vào
                if (request.OriginalImportId <= 0)
                {
                    Console.WriteLine("OriginalImportId không hợp lệ.");
                    throw new ArgumentException("OriginalImportId không hợp lệ.");
                }
                if (request.ImportDetails == null || !request.ImportDetails.Any())
                {
                    Console.WriteLine("Không có thông tin import detail.");
                    throw new ArgumentException("Không có thông tin import detail.");
                }

                // Lấy đơn cũ với eager loading các chi tiết
                var oldImport = await _impRepos.GetByIdAsyncWithDetails(request.OriginalImportId);
                if (oldImport == null)
                {
                    Console.WriteLine("Đơn cũ không tồn tại.");
                    throw new ArgumentException("Đơn cũ không tồn tại.");
                }

                // Map request sang entity Import (đơn bổ sung)
                var newImport = _mapper.Map<Import>(request);
                newImport.CreatedBy = oldImport.CreatedBy;
                newImport.ReferenceNumber = oldImport.ReferenceNumber;
                newImport.CreatedDate = DateTime.UtcNow;
                newImport.Status = "Approved";
                newImport.ApprovedDate = null;
                newImport.CompletedDate = null;
                newImport.OriginalImportId = oldImport.ImportId;

                decimal totalCost = 0;
                newImport.ImportDetails = new List<ImportDetail>();

                // Duyệt qua từng ImportDetail của đơn cũ để tạo mới
                foreach (var oldDetail in oldImport.ImportDetails)
                {
                    var missingStores = oldDetail.ImportStoreDetails?
                        .Where(s => (s.ActualReceivedQuantity ?? 0) < s.AllocatedQuantity)
                        .ToList();

                    if (missingStores == null || !missingStores.Any())
                    {
                        Console.WriteLine($"Không còn store thiếu cho sản phẩm có ProductVariantId {oldDetail.ProductVariantId}. Bỏ qua bước này.");
                        continue;
                    }

                    var reqDetail = request.ImportDetails.FirstOrDefault(d => d.ProductVariantId == oldDetail.ProductVariantId);
                    if (reqDetail == null)
                    {
                        Console.WriteLine($"Thiếu thông tin unitPrice cho sản phẩm có ProductVariantId {oldDetail.ProductVariantId}");
                        throw new ArgumentException($"Thiếu thông tin unitPrice cho sản phẩm có ProductVariantId {oldDetail.ProductVariantId}");
                    }

                    int totalMissing = 0;
                    var newDetail = new ImportDetail
                    {
                        ProductVariantId = oldDetail.ProductVariantId,
                        Quantity = 0, // sẽ cập nhật sau
                        CostPrice = 0,
                        ImportStoreDetails = new List<ImportStoreDetail>()
                    };

                    // Duyệt qua các store thiếu để tạo mới ImportStoreDetail
                    foreach (var store in missingStores)
                    {
                        int actualReceived = store.ActualReceivedQuantity ?? 0;
                        int missing = store.AllocatedQuantity - actualReceived;
                        totalMissing += missing;

                        newDetail.ImportStoreDetails.Add(new ImportStoreDetail
                        {
                            ActualReceivedQuantity = null,
                            AllocatedQuantity = missing,
                            Status = "Pending",
                            StaffDetailId = store.StaffDetailId,
                            WarehouseId = store.WarehouseId,
                            HandleBy = store.HandleBy
                        });
                    }

                    newDetail.Quantity = totalMissing;
                    if (totalMissing > 0)
                    {
                        newImport.ImportDetails.Add(newDetail);
                        totalCost += totalMissing * reqDetail.CostPrice;
                        Console.WriteLine($"Đã tạo ImportDetail cho sản phẩm có ProductVariantId {oldDetail.ProductVariantId} với số lượng thiếu = {totalMissing}.");

                        // Cập nhật trạng thái của các ImportStoreDetails cũ thành "Handled"
                        foreach (var store in missingStores)
                        {
                            store.Status = "Handled";
                        }
                    }
                }

                newImport.TotalCost = totalCost;

                // Lưu đơn bổ sung mới qua repository
                _impRepos.Add(newImport);
                await _impRepos.SaveChangesAsync();
                Console.WriteLine("Lưu đơn bổ sung mới thành công qua repository.");

                // Tạo AuditLog cho đơn mới
                var serializedNewImport = JsonConvert.SerializeObject(newImport,
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                var auditLogNew = new AuditLog
                {
                    TableName = "Import",
                    RecordId = newImport.ImportId.ToString(),
                    Operation = "CREATE",
                    ChangeDate = DateTime.UtcNow,
                    ChangedBy = oldImport.CreatedBy,
                    ChangeData = serializedNewImport,
                    Comment = "Tạo mới đơn import bổ sung dựa trên đơn cũ"
                };
                _auditLogRepos.Add(auditLogNew);
                await _auditLogRepos.SaveChangesAsync();
                Console.WriteLine("AuditLog cho đơn mới đã được tạo thành công.");

                // Cập nhật trạng thái cho đơn cũ
                oldImport.Status = "Supplement Created";
                await _impRepos.SaveChangesAsync();
                Console.WriteLine("Cập nhật trạng thái của đơn cũ thành 'Supplement Created' thành công.");

                var serializedOldImport = JsonConvert.SerializeObject(oldImport,
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                var auditLogOld = new AuditLog
                {
                    TableName = "Import",
                    RecordId = oldImport.ImportId.ToString(),
                    Operation = "UPDATE",
                    ChangeDate = DateTime.UtcNow,
                    ChangedBy = oldImport.CreatedBy,
                    ChangeData = serializedOldImport,
                    Comment = "Cập nhật đơn cũ với status 'Supplement Created'"
                };
                _auditLogRepos.Add(auditLogOld);
                await _auditLogRepos.SaveChangesAsync();
                Console.WriteLine("AuditLog cho đơn cũ đã được tạo thành công.");

                // Map entity sang DTO response
                var resultDto = _mapper.Map<ImportDto>(newImport);
                Console.WriteLine("Mapping từ entity sang DTO response thành công.");

                // Tải lại đầy đủ dữ liệu (nếu cần) để tạo báo cáo
                var supplementImportEntity = await _impRepos.GetByIdAsync(newImport.ImportId);
                var oldImportEntity = await _impRepos.GetByIdAsync(oldImport.ImportId);
                if (supplementImportEntity == null || oldImportEntity == null)
                {
                    Console.WriteLine("Không tìm thấy dữ liệu đơn nhập khi tạo báo cáo.");
                    throw new Exception("Không tìm thấy dữ liệu đơn nhập khi tạo báo cáo.");
                }

                // Gọi ReportService để tạo báo cáo nhập bổ sung
                byte[] reportFileBytes = _reportService.GenerateImportSupplementSlip(supplementImportEntity, oldImportEntity);
                string reportBase64 = Convert.ToBase64String(reportFileBytes);

                // Tạo DTO kết hợp ImportDto và chuỗi báo cáo
                var resultWithReport = new ImportSupplementReportDto
                {
                    ImportData = resultDto,
                    ReportFileBase64 = reportBase64
                };

                return new ResponseDTO<ImportSupplementReportDto>(resultWithReport, true, "Tạo đơn import bổ sung thành công!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Quá trình tạo import bổ sung thất bại: {ex.Message}");
                return new ResponseDTO<ImportSupplementReportDto>(null, false, $"Quá trình tạo import bổ sung thất bại: {ex.Message}");
            }
        }

    }
}
