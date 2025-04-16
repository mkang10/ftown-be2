using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.UseCases.TransferHandler;

namespace Application.UseCases
{

    public class TransferHandler
    {
        private readonly ITransferRepos _transferRepos;
        private readonly IDispatchRepos _dispatchRepos;
        private readonly IImportRepos _importRepos;
        private readonly IStoreExportRepos _storeExportRepos;
        private readonly IImportStoreRepos _importStoreRepos;
        private readonly IAuditLogRepos _auditLogRepos;
        private readonly IMapper _mapper;

        public TransferHandler(
            ITransferRepos transferRepos,
            IDispatchRepos dispatchRepos,
            IImportRepos importRepos,
            IStoreExportRepos storeExportRepos,
            IImportStoreRepos importStoreRepos,
            IAuditLogRepos auditLogRepos,
            IMapper mapper)
        {
            _transferRepos = transferRepos;
            _dispatchRepos = dispatchRepos;
            _importRepos = importRepos;
            _storeExportRepos = storeExportRepos;
            _importStoreRepos = importStoreRepos;
            _auditLogRepos = auditLogRepos;
            _mapper = mapper;
        }

        public async Task<ResponseDTO<TransferFullFlowDto>> CreateTransferFullFlowAsync(CreateTransferFullFlowDto request)
        {
            // 1. Tạo đối tượng Transfer từ request (chưa lưu vào DB)
            var newTransfer = _mapper.Map<Transfer>(request);
            newTransfer.CreatedDate = DateTime.UtcNow;
            newTransfer.Status = "Approved";
            foreach (var detailDto in request.TransferDetails)
            {
                var newTransferDetail = _mapper.Map<TransferDetail>(detailDto);
                newTransfer.TransferDetails.Add(newTransferDetail);
            }

            // 2. Tạo Dispatch và lưu để có DispatchId
            var newDispatch = await CreateDispatchAsync(request, newTransfer);

            // 3. Tạo Import và lưu để có ImportId
            var newImport = await CreateImportAsync(request, newTransfer);

            // 4. Gán DispatchId và ImportId cho Transfer, sau đó lưu Transfer
            newTransfer.DispatchId = newDispatch.DispatchId;
            newTransfer.ImportId = newImport.ImportId;
            _transferRepos.Add(newTransfer);
            await _transferRepos.SaveChangesAsync();
            await LogAuditAsync("Transfer", "CREATE", newTransfer.TransferOrderId, request.CreatedBy, newTransfer,
                "Tạo mới đơn chuyển hàng");
            foreach (var detail in newTransfer.TransferDetails)
            {
                await LogAuditAsync("TransferDetail", "CREATE", detail.TransferOrderDetailId, request.CreatedBy, detail,
                    "Tạo chi tiết đơn chuyển hàng");
            }

            // 5. Tạo các bản ghi StoreExport cho từng DispatchDetail
            await CreateStoreExportRecordsAsync(request, newDispatch);

            // 6. Mapping kết quả trả về
            var resultDto = _mapper.Map<TransferFullFlowDto>(newTransfer);
            return new ResponseDTO<TransferFullFlowDto>(resultDto, true, "Tạo đơn chuyển hàng và các đơn liên quan thành công!");
        }

        #region Helper Methods

        private async Task<Dispatch> CreateDispatchAsync(CreateTransferFullFlowDto request, Transfer transfer)
        {
            var dispatch = new Dispatch
            {
                CreatedBy = request.CreatedBy,
                CreatedDate = DateTime.UtcNow,
                Status = "Approved",
                ReferenceNumber = !string.IsNullOrEmpty(request.DispatchReferenceNumber) && request.DispatchReferenceNumber.StartsWith("DIS")
                                    ? request.DispatchReferenceNumber
                                    : "DIS" + new Random().Next(100, 1000).ToString(),
                Remarks = "Tự động tạo từ chuyển hàng"
            };

            // Tạo DispatchDetail từ các TransferDetail
            foreach (var transferDetail in transfer.TransferDetails)
            {
                var dispatchDetail = new DispatchDetail
                {
                    VariantId = transferDetail.VariantId,
                    Quantity = transferDetail.Quantity

                };
                dispatch.DispatchDetails.Add(dispatchDetail);
            }
            _dispatchRepos.Add(dispatch);
            await _dispatchRepos.SaveChangesAsync();

            // Audit cho Dispatch và các DispatchDetail
            await LogAuditAsync("Dispatch", "CREATE", dispatch.DispatchId, request.CreatedBy, dispatch, "Tạo mới đơn xuất hàng");
            foreach (var detail in dispatch.DispatchDetails)
            {
                await LogAuditAsync("DispatchDetail", "CREATE", detail.DispatchDetailId, request.CreatedBy, detail, "Tạo chi tiết đơn xuất hàng");
            }
            return dispatch;
        }

        private async Task<Import> CreateImportAsync(CreateTransferFullFlowDto request, Transfer transfer)
        {
            // 1. Tính tổng chi phí
            decimal totalCost = request.TransferDetails.Sum(d => d.Quantity * d.CostPrice);

            // 2. Tạo đối tượng Import mà không gán ImportID (để EF tự sinh giá trị)
            var import = new Import
            {
                CreatedBy = request.CreatedBy,
                CreatedDate = DateTime.UtcNow,
                Status = "Approved",
                ReferenceNumber = !string.IsNullOrEmpty(request.ImportReferenceNumber) && request.ImportReferenceNumber.StartsWith("IIN")
                                    ? request.ImportReferenceNumber
                                    : "IIN" + new Random().Next(100, 1000).ToString(),
                TotalCost = totalCost,
                ApprovedDate = null,
                CompletedDate = null
            };

            // 3. Tạo ImportDetail từ từng TransferDetail và thêm vào collection của Import
            foreach (var transferDetail in transfer.TransferDetails)
            {
                var importDetail = new ImportDetail
                {
                    ProductVariantId = transferDetail.VariantId,
                    Quantity = transferDetail.Quantity,
                    CostPrice = 0,
                    // Thiết lập quan hệ: EF sẽ tự gán ImportID cho ImportDetail thông qua navigation property
                    Import = import
                };
                import.ImportDetails.Add(importDetail);
            }

            // 4. Thêm Import (với các ImportDetail) vào repository và lưu để sinh ra các giá trị identity
            _importRepos.Add(import);
            await _importRepos.SaveChangesAsync(); // Sau bước này, import.ImportID và importDetail.ImportDetailID đã có giá trị từ DB

            // 5. Lấy thông tin Warehouse để lấy HandleBy (ShopManagerId) từ Warehouse
            // Giả sử bạn có WarehouseRepository với hàm GetByIdAsync
            var warehouse = await _importRepos.GetWareHouseByIdAsync(request.DestinationWarehouseId);
            int? handleBy = warehouse?.ShopManagerId;

            // 6. Tạo ImportStoreDetail cho mỗi ImportDetail
            foreach (var importDetail in import.ImportDetails)
            {
                var importStore = new ImportStoreDetail
                {
                    AllocatedQuantity = importDetail.Quantity,
                    Status = "Pending",
                    Comments = "Nhập hàng vào warehouse đích cho chuyển hàng",
                    ImportDetail = importDetail,   // Liên kết ImportStoreDetail với ImportDetail
                    WarehouseId = request.DestinationWarehouseId,
                    HandleBy = handleBy             // Lấy từ Warehouse
                };

                _importStoreRepos.Add(importStore);
                // Bạn có thể lưu một lần sau vòng lặp (nếu không cần log ngay từng dòng)
                await _importStoreRepos.SaveChangesAsync();

                // Log cho ImportStoreDetail với identity đã được sinh ra
                await LogAuditAsync("ImportStoreDetail", "CREATE", importStore.ImportStoreId, request.CreatedBy, importStore,
                    "Tạo bản ghi nhập hàng");
            }

            // 7. Log Audit cho Import và từng ImportDetail
            await LogAuditAsync("Import", "CREATE", import.ImportId, request.CreatedBy, import, "Tạo mới đơn nhập hàng");
            foreach (var detail in import.ImportDetails)
            {
                await LogAuditAsync("ImportDetail", "CREATE", detail.ImportDetailId, request.CreatedBy, detail, "Tạo chi tiết đơn nhập hàng");
            }

            return import;
        }




        private async Task CreateStoreExportRecordsAsync(CreateTransferFullFlowDto request, Dispatch dispatch)
        {
            // Tạo danh sách để lưu các bản ghi StoreExportStoreDetail
            var storeExports = new List<StoreExportStoreDetail>();

            // Lấy thông tin Warehouse nguồn để lấy HandleBy (ShopManagerId)
            var warehouse = await _importRepos.GetWareHouseByIdAsync(request.SourceWarehouseId);
            var handleBy = warehouse?.ShopManagerId;

            foreach (var dispatchDetail in dispatch.DispatchDetails)
            {
                var storeExport = new StoreExportStoreDetail
                {
                    // Gán DispatchDetailId lấy từ đối tượng dispatchDetail
                    DispatchDetailId = dispatchDetail.DispatchDetailId,
                    WarehouseId = request.SourceWarehouseId,
                    AllocatedQuantity = dispatchDetail.Quantity,
                    Status = "Pending",
                    Comments = "Xuất hàng từ warehouse nguồn cho chuyển hàng",
                    HandleBy = handleBy
                };

                _storeExportRepos.Add(storeExport);
                storeExports.Add(storeExport);
            }

            // Lưu các bản ghi và sau đó log với giá trị DispatchStoreDetailId đã được sinh ra
            await _storeExportRepos.SaveChangesAsync();

            foreach (var storeExport in storeExports)
            {
                await LogAuditAsync("StoreExportStoreDetail", "CREATE", storeExport.DispatchStoreDetailId, request.CreatedBy, storeExport,
                    "Tạo bản ghi xuất hàng");
            }
        }


        private async Task LogAuditAsync(string tableName, string operation, int recordId, int changedBy, object entity, string comment)
        {
            var auditLog = new AuditLog
            {
                TableName = tableName,
                RecordId = recordId.ToString(),
                Operation = operation,
                ChangeDate = DateTime.UtcNow,
                ChangedBy = changedBy,
                ChangeData = JsonConvert.SerializeObject(entity, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                Comment = comment
            };
            _auditLogRepos.Add(auditLog);
            await _auditLogRepos.SaveChangesAsync();
        }

        #endregion
    }
}