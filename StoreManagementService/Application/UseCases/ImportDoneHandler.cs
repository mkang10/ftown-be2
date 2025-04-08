using Domain.DTO.Request;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class ImportDoneHandler
    {
        private readonly IImportRepos _importRepos;
        private readonly IAuditLogRepos _auditLogRepos;
        private readonly IWareHouseStockRepos _wareHouseStockRepos;

        public ImportDoneHandler(IWareHouseStockRepos wareHouseStockRepos, IImportRepos importRepos, IAuditLogRepos auditLogRepos)
        {
            _importRepos = importRepos;
            _auditLogRepos = auditLogRepos;
            _wareHouseStockRepos = wareHouseStockRepos;
        }



        public async Task ProcessImportDoneAsync(int importId, int staffId, List<UpdateStoreDetailDto> confirmations)
        {
            // Lấy Import với các ImportDetail và ImportStoreDetails đã được include
            var import = await _importRepos.GetByIdAssignAsync(importId);
            if (import == null)
            {
                throw new Exception("Import không tồn tại");
            }

            // Kiểm tra trạng thái Import có cho phép cập nhật không
            ValidateImportStatus(import);

            // Cập nhật các ImportStoreDetail dựa trên thông tin từ confirmations và reload lại Import để cập nhật navigation properties
            await UpdateStoreDetailsAndReloadAsync(import, confirmations, staffId);

            // Kiểm tra nếu tất cả các ImportStoreDetail đều đạt trạng thái "Success" thì cập nhật Import thành "Done"
            if (await TryUpdateImportStatusToDoneAsync(import, staffId))
            {
                // Nếu Import này là đơn bổ sung thì cập nhật số lượng thực nhận của đơn gốc
                await UpdateOriginalImportFromSupplementsAsync(import, staffId);

                // Cập nhật kho theo toàn bộ Import
                await _wareHouseStockRepos.UpdateWarehouseStockAsync(import, staffId);
            }
            else
            {
                // Nếu không phải tất cả các ImportStoreDetail đều "Success"
                // thì cập nhật Warehouse cho các ImportStoreDetail thành công riêng lẻ
                // và cập nhật trạng thái Import thành "Success" hoặc "Partial Success"
                await UpdateWarehouseForSuccessDetailsAsync(import, staffId);
            }

            // Lưu các thay đổi: Import, WarehouseStock và AuditLog
            await _importRepos.SaveChangesAsync();
            await _auditLogRepos.SaveChangesAsync();

            // Cập nhật Transfer nếu có liên quan
            await UpdateTransferStatusAsync(import, staffId);
        }

        #region Helper Methods

        /// <summary>
        /// Kiểm tra trạng thái Import có cho phép cập nhật không.
        /// Chỉ cho phép khi trạng thái là "Processing" hoặc "Partial Success".
        /// </summary>
        private void ValidateImportStatus(Import import)
        {
            var currentStatus = import.Status.Trim();
            if (!string.Equals(currentStatus, "Processing", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(currentStatus, "Partial Success", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(currentStatus, "Supplement Created", StringComparison.OrdinalIgnoreCase))

            {
                throw new InvalidOperationException("Chỉ cho phép chỉnh sửa các Import có trạng thái Processing , Partial Success và Supplement Created");
            }
        }

        /// <summary>
        /// Cập nhật ImportStoreDetail theo danh sách confirmations, sau đó lưu và reload lại Import.
        /// </summary>
        private async Task UpdateStoreDetailsAndReloadAsync(Import import, List<UpdateStoreDetailDto> confirmations, int staffId)
        {
            await UpdateImportStoreDetails(import, confirmations, staffId);
            await _importRepos.SaveChangesAsync();
            await _importRepos.ReloadAsync(import);
        }

        /// <summary>
        /// Kiểm tra nếu tất cả các ImportStoreDetail đều đạt trạng thái "Success"
        /// thì cập nhật Import thành "Done" và tạo AuditLog.
        /// Trả về true nếu Import được cập nhật thành "Done".
        /// </summary>
        private async Task<bool> TryUpdateImportStatusToDoneAsync(Import import, int staffId)
        {
            var storeDetails = import.ImportDetails.SelectMany(d => d.ImportStoreDetails).ToList();
            var successDetails = storeDetails
                                 .Where(sd => string.Equals(sd.Status.Trim(), "Success", StringComparison.OrdinalIgnoreCase))
                                 .ToList();

            if (storeDetails.Count > 0 && storeDetails.Count == successDetails.Count &&
                !string.Equals(import.Status.Trim(), "Done", StringComparison.OrdinalIgnoreCase))
            {
                UpdateImportStatusToDone(import, staffId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Cập nhật Warehouse cho từng ImportStoreDetail có trạng thái "Success".
        /// Sau đó cập nhật trạng thái Import thành "Success" hoặc "Partial Success" tùy vào tổng số.
        /// </summary>
        private async Task UpdateWarehouseForSuccessDetailsAsync(Import import, int staffId)
        {
            var storeDetails = import.ImportDetails.SelectMany(d => d.ImportStoreDetails).ToList();
            var successDetails = storeDetails
                                 .Where(sd => string.Equals(sd.Status.Trim(), "Success", StringComparison.OrdinalIgnoreCase))
                                 .ToList();

            if (successDetails.Any())
            {
                foreach (var detail in successDetails)
                {
                    await _wareHouseStockRepos.UpdateWarehouseStockForSingleDetailAsync(detail, detail.ImportDetail.ProductVariantId, staffId);
                }

                // Ví dụ: nếu tổng số ImportStoreDetail là 2 và cả 2 đều thành công,
                // cập nhật Import thành "Success"; ngược lại là "Partial Success"
                if (storeDetails.Count == 2 && successDetails.Count == 2)
                {
                    import.Status = "Success";
                }
                else
                {
                    import.Status = "Partial Success";
                }
            }
        }

        /// <summary>
        /// Nếu Import có trường OriginalImportId (đơn bổ sung), cập nhật số lượng của đơn gốc.
        /// Duyệt qua các đơn bổ sung đã Done, cập nhật số lượng thực nhận của ImportStoreDetail
        /// trong đơn gốc và nếu đạt đủ số lượng thì cập nhật trạng thái thành "Done".
        /// </summary>
        private async Task UpdateOriginalImportFromSupplementsAsync(Import import, int staffId)
        {
            if (import.OriginalImportId.HasValue && import.OriginalImportId.Value > 0)
            {
                var supplementImports = await _importRepos.GetAllByOriginalImportIdAsync(import.OriginalImportId.Value);
                if (supplementImports != null &&
                    supplementImports.All(i => string.Equals(i.Status.Trim(), "Done", StringComparison.OrdinalIgnoreCase)))
                {
                    var originalImport = await _importRepos.GetByIdAsync(import.OriginalImportId.Value);
                    if (originalImport != null && !string.Equals(originalImport.Status.Trim(), "Done", StringComparison.OrdinalIgnoreCase))
                    {
                        UpdateOriginalImportStoreDetails(originalImport, supplementImports);
                        UpdateImportStatusToDone(originalImport, staffId);
                    }
                }
            }
        }

        /// <summary>
        /// Duyệt qua các đơn bổ sung và cập nhật số lượng ActualReceivedQuantity của các ImportStoreDetail
        /// trong đơn gốc dựa trên số lượng từ các đơn bổ sung.
        /// Nếu sau khi cộng đủ, cập nhật trạng thái ImportStoreDetail thành "Done".
        /// </summary>
        private void UpdateOriginalImportStoreDetails(Import originalImport, IEnumerable<Import> supplementImports)
        {
            foreach (var suppImport in supplementImports)
            {
                foreach (var suppDetail in suppImport.ImportDetails)
                {
                    // Tìm ImportDetail tương ứng trong đơn gốc theo ProductVariantId
                    var originalDetail = originalImport.ImportDetails
                        .FirstOrDefault(od => od.ProductVariantId == suppDetail.ProductVariantId);
                    if (originalDetail != null)
                    {
                        foreach (var suppStore in suppDetail.ImportStoreDetails)
                        {
                            // Tìm ImportStoreDetail tương ứng trong ImportDetail của đơn gốc dựa trên StaffDetailId và WarehouseId
                            var originalStore = originalDetail.ImportStoreDetails
                                .FirstOrDefault(os => os.StaffDetailId == suppStore.StaffDetailId &&
                                                      os.WarehouseId == suppStore.WarehouseId);
                            if (originalStore != null)
                            {
                                // Cộng dồn số lượng thực nhận từ đơn bổ sung vào ImportStoreDetail gốc
                                originalStore.ActualReceivedQuantity = (originalStore.ActualReceivedQuantity ?? 0) + (suppStore.ActualReceivedQuantity ?? 0);

                                // Nếu sau khi cộng dồn đạt hoặc vượt quá AllocatedQuantity thì cập nhật trạng thái thành "Done"
                                if ((originalStore.ActualReceivedQuantity ?? 0) >= originalStore.AllocatedQuantity)
                                {
                                    originalStore.Status = "Done";
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Cập nhật trạng thái Transfer nếu Transfer có chứa Import hiện tại và Dispatch cũng đã Done.
        /// </summary>
        private async Task UpdateTransferStatusAsync(Import import, int staffId)
        {
            var transfer = await _importRepos.GetTransferByImportIdAsync(import.ImportId);
            if (transfer != null)
            {
                bool importDone = transfer.Import != null &&
                                  string.Equals(transfer.Import.Status.Trim(), "Done", StringComparison.OrdinalIgnoreCase);
                bool dispatchDone = transfer.Dispatch != null &&
                                    string.Equals(transfer.Dispatch.Status.Trim(), "Done", StringComparison.OrdinalIgnoreCase);

                if (importDone && dispatchDone && !string.Equals(transfer.Status.Trim(), "Done", StringComparison.OrdinalIgnoreCase))
                {
                    transfer.Status = "Done";
                    var auditLogTransfer = new AuditLog
                    {
                        TableName = "Transfer",
                        RecordId = transfer.TransferOrderId.ToString(),
                        Operation = "UPDATE",
                        ChangeDate = DateTime.Now,
                        ChangedBy = staffId,
                        ChangeData = "Status updated to Done",
                        Comment = "Both Import and Dispatch have status Done"
                    };
                    _auditLogRepos.Add(auditLogTransfer);

                    await _importRepos.SaveChangesAsync();
                    await _auditLogRepos.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Cập nhật Import thành "Done" bằng cách đặt trạng thái, cập nhật CompletedDate
        /// và tạo AuditLog ghi lại thay đổi.
        /// </summary>
        private void UpdateImportStatusToDone(Import import, int staffId)
        {
            import.Status = "Done";
            import.CompletedDate = DateTime.UtcNow;

            var serializedImport = JsonConvert.SerializeObject(import,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            var auditLog = new AuditLog
            {
                TableName = "Import",
                RecordId = import.ImportId.ToString(),
                Operation = "UPDATE",
                ChangeDate = DateTime.UtcNow,
                ChangedBy = staffId,
                ChangeData = serializedImport,
                Comment = "Cập nhật Import thành Done"
            };

            _auditLogRepos.Add(auditLog);
        }

        /// <summary>
        /// Cập nhật từng ImportStoreDetail dựa trên danh sách xác nhận từ UpdateStoreDetailDto.
        /// Nếu số lượng thực nhận đạt hoặc vượt AllocatedQuantity, cập nhật trạng thái thành "Success",
        /// ngược lại là "Partial Success".
        /// </summary>
        private async Task UpdateImportStoreDetails(Import import, List<UpdateStoreDetailDto> confirmations, int staffId)
        {
            foreach (var confirmation in confirmations)
            {
                // Tìm ImportStoreDetail trong đơn hiện tại dựa trên StoreDetailId
                var storeDetail = import.ImportDetails
                    .SelectMany(d => d.ImportStoreDetails)
                    .FirstOrDefault(sd => sd.ImportStoreId == confirmation.StoreDetailId);

                if (storeDetail != null)
                {
                    // Cập nhật trạng thái Success
                    storeDetail.ActualReceivedQuantity = storeDetail.AllocatedQuantity;
                    storeDetail.Status = "Success";

                    // Truyền theo thông tin chung (liên kết logic) thay vì ImportStoreId
                    await PropagateSuccessToAncestorsAsync(
                        import,
                        (int)storeDetail.WarehouseId,
                        (int)storeDetail.StaffDetailId,
                        staffId
                    );
                }
                else
                {
                    throw new Exception($"Không tìm thấy ImportStoreDetail với ID {confirmation.StoreDetailId}");
                }
            }

            await Task.CompletedTask;
        }


        /// <summary>
        /// Hàm đệ quy truyền trạng thái Success cho các đơn cha của import hiện tại.
        /// Giả sử mỗi đơn con có trường OriginalImportId trỏ đến đơn cha của nó.
        /// Chúng ta sẽ load đơn cha, tìm store detail tương ứng (theo StoreDetailId) và cập nhật thành Success.
        /// Sau đó, tiếp tục truyền cho đơn cha của đơn cha.
        /// </summary>
        private async Task PropagateSuccessToAncestorsAsync(
     Import childImport,
     int warehouseId,
     int staffDetailId,
     int staffId)
        {
            if (childImport.OriginalImportId.HasValue && childImport.OriginalImportId.Value > 0)
            {
                var parentImport = await _importRepos.GetByIdAsync(childImport.OriginalImportId.Value);
                if (parentImport != null)
                {
                    var parentStoreDetail = parentImport.ImportDetails
                        .SelectMany(d => d.ImportStoreDetails)
                        .FirstOrDefault(sd =>
                            sd.WarehouseId == warehouseId &&
                            sd.StaffDetailId == staffDetailId
                        );

                    if (parentStoreDetail != null)
                    {
                        parentStoreDetail.ActualReceivedQuantity = parentStoreDetail.AllocatedQuantity;
                        parentStoreDetail.Status = "Success";
                    }

                    var parentStoreDetails = parentImport.ImportDetails
                        .SelectMany(d => d.ImportStoreDetails)
                        .ToList();

                    if (parentStoreDetails.Any() && parentStoreDetails.All(sd =>
                            string.Equals(sd.Status.Trim(), "Success", StringComparison.OrdinalIgnoreCase)))
                    {
                        parentImport.Status = "Done";
                    }

                    // Đệ quy tiếp tục lên ông nội
                    await PropagateSuccessToAncestorsAsync(
                        parentImport,
                        warehouseId,
                        staffDetailId,
                        staffId
                    );
                }
            }
        }

    }
}
    #endregion