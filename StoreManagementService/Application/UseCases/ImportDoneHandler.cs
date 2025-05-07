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
        private readonly IStaffDetailRepository _staffDetail;


        public ImportDoneHandler(IStaffDetailRepository staffDetail, IWareHouseStockRepos wareHouseStockRepos, IImportRepos importRepos, IAuditLogRepos auditLogRepos)
        {
            _importRepos = importRepos;
            _auditLogRepos = auditLogRepos;
            _wareHouseStockRepos = wareHouseStockRepos;
            _staffDetail = staffDetail;
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
            await _importRepos.SaveChangesAsync();

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
            if (string.Equals(import.ImportType?.Trim(), "Purchase", StringComparison.OrdinalIgnoreCase))
            {
                await UpdateVariantPricesAsync(import, staffId);
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
            if (!string.Equals(currentStatus, "Approved", StringComparison.OrdinalIgnoreCase) &&
                                !string.Equals(currentStatus, "Shortage", StringComparison.OrdinalIgnoreCase) &&
                                                               

                !string.Equals(currentStatus, "Partial Success", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(currentStatus, "Supplement Created", StringComparison.OrdinalIgnoreCase))

            {
                throw new InvalidOperationException("Chỉ cho phép chỉnh sửa các Import có trạng thái Approved , Partial Success, Shortage  và Supplement Created");
            }
        }

        /// <summary>
        /// Cập nhật ImportStoreDetail theo danh sách confirmations, sau đó lưu và reload lại Import.
        /// </summary>
        private async Task UpdateStoreDetailsAndReloadAsync(Import import, List<UpdateStoreDetailDto> confirmations, int staffId)
        {
            // Kiểm tra Dispatch trước khi cho phép hoàn thành Import
            var transfer = await _importRepos.GetTransferByImportIdAsync(import.ImportId);
            if (transfer != null
                && transfer.Dispatch != null
                && !string.Equals(transfer.Dispatch.Status?.Trim(), "Done", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Không thể hoàn thành đơn nhập hàng vì 1 hoặc nhiều đơn xuất hàng chưa được gửi đi");
            }

            // Nếu qua được kiểm tra thì thực hiện cập nhật
            await UpdateImportStoreDetails(import, confirmations, staffId);
            await _importRepos.SaveChangesAsync();

            // Reload entity để lấy trạng thái mới nhất
            await _importRepos.ReloadAsync(import);
        }


        /// <summary>
        /// Kiểm tra nếu tất cả các ImportStoreDetail đều đạt trạng thái "Success"
        /// thì cập nhật Import thành "Done" và tạo AuditLog.
        /// Trả về true nếu Import được cập nhật thành "Done".
        /// </summary>
        private async Task<bool> TryUpdateImportStatusToDoneAsync(Import import, int staffId)
        {
           

            var storeDetails = import.ImportDetails
                                     .SelectMany(d => d.ImportStoreDetails)
                                     .ToList();
            var successDetails = storeDetails
                                 .Where(sd => string.Equals(sd.Status?.Trim(), "Success", StringComparison.OrdinalIgnoreCase))
                                 .ToList();

            if (storeDetails.Count > 0
                && storeDetails.Count == successDetails.Count
                && !string.Equals(import.Status?.Trim(), "Done", StringComparison.OrdinalIgnoreCase))
            {
                // ==== MỚI: kiểm tra Dispatch đã done chưa ====
               
                // ============================================

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
            // Lấy tất cả các ImportStoreDetail từ import
            var storeDetails = import.ImportDetails
                                     .SelectMany(d => d.ImportStoreDetails)
                                     .ToList();

            // Lọc ra những detail thực sự success
            var successDetails = storeDetails
                .Where(sd =>
                    string.Equals(sd.Status?.Trim(),
                                  "Success",
                                  StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!storeDetails.Any())
            {
                // Nếu không có detail nào, bạn có thể giữ nguyên trạng thái
                // hoặc gán import.Status = "No Details";
                return;
            }

            // Cập nhật kho cho từng detail success
            foreach (var detail in successDetails)
            {
                await _wareHouseStockRepos
                    .UpdateWarehouseStockForSingleDetailAsync(
                        detail,
                        detail.ImportDetail.ProductVariantId,
                        staffId);
            }

            // Nếu số lượng successDetails == tổng số storeDetails ⇒ Success
            // Ngược lại ⇒ Partial Success
            import.Status = (successDetails.Count == storeDetails.Count)
                ? "Done"
                : "Partial Success";
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
            var accountId = await _staffDetail.GetAccountIdByStaffIdAsync(staffId);
            if (accountId == null)
                throw new KeyNotFoundException($"Không tìm thấy Account cho StaffId={staffId}");
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
                        ChangedBy = accountId,
                        ChangeData = "Status updated to Done",
                        Comment = "Đơn chuyển hàng đã hoàn thành !"
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
        private async Task UpdateImportStatusToDone(Import import, int staffId)
        {

            var accountId = await _staffDetail.GetAccountIdByStaffIdAsync(staffId);
            if (accountId == null)
                throw new KeyNotFoundException($"Không tìm thấy Account cho StaffId={staffId}");
            import.Status = "Done";
            import.CompletedDate = DateTime.Now;

            var serializedImport = JsonConvert.SerializeObject(import,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            var auditLog = new AuditLog
            {
                TableName = "Import",
                RecordId = import.ImportId.ToString(),
                Operation = "UPDATE",
                ChangeDate = DateTime.Now,
                ChangedBy = accountId,
                ChangeData = serializedImport,
                Comment = "Cập nhật đơn xuất hàng thành Done"
            };

            _auditLogRepos.Add(auditLog);
        }

        /// <summary>
        /// Cập nhật từng ImportStoreDetail dựa trên danh sách xác nhận từ UpdateStoreDetailDto.
        /// Nếu số lượng thực nhận đạt hoặc vượt AllocatedQuantity, cập nhật trạng thái thành "Success",
        /// ngược lại là "Partial Success".
        /// </summary>
        private async Task UpdateImportStoreDetails(
    Import import,
    List<UpdateStoreDetailDto> confirmations,
    int staffId)
        {
            // Chuyển từ staffId sang accountId
            var accountId = await _staffDetail.GetAccountIdByStaffIdAsync(staffId);
            if (accountId == null)
                throw new KeyNotFoundException($"Không tìm thấy Account cho StaffId={staffId}");

            foreach (var confirmation in confirmations)
            {
                // Tìm ImportStoreDetail
                var storeDetail = import.ImportDetails
                    .SelectMany(d => d.ImportStoreDetails)
                    .FirstOrDefault(sd => sd.ImportStoreId == confirmation.StoreDetailId);

                if (storeDetail == null)
                    throw new Exception($"Không tìm thấy ImportStoreDetail với ID {confirmation.StoreDetailId}");

                // Cập nhật trạng thái
                storeDetail.ActualReceivedQuantity = storeDetail.AllocatedQuantity;
                storeDetail.Status = "Success";

                // Tạo AuditLog cho ImportStoreDetail
                var auditLogDetail = new AuditLog
                {
                    TableName = "ImportStoreDetail",
                    RecordId = storeDetail.ImportStoreId.ToString(),
                    Operation = "UPDATE",
                    ChangeDate = DateTime.Now,
                    ChangedBy = accountId,
                    ChangeData = $"Status: Success, ActualReceivedQuantity: {storeDetail.ActualReceivedQuantity}",
                    Comment = "Đơn hàng đã được nhập vào kho thành công !"
                };
                _auditLogRepos.Add(auditLogDetail);

                // Truyền tiếp success lên các đối tượng liên quan
                await PropagateSuccessToAncestorsAsync(
                    import,
                    (int)storeDetail.WarehouseId,
                    (int)storeDetail.StaffDetailId,
                    staffId
                );
            }
        }
        private async Task UpdateVariantPricesAsync(Import import, int staffId)
        {
            // Lấy danh sách variant đã có trong import này
            var variantIds = import.ImportDetails
                                   .Select(d => d.ProductVariantId)
                                   .Distinct();

            foreach (var variantId in variantIds)
            {
                var accountId = await _staffDetail.GetAccountIdByStaffIdAsync(staffId);
                if (accountId == null)
                    throw new KeyNotFoundException($"Không tìm thấy Account cho StaffId={staffId}");
                // 1) Lấy tất cả ImportDetail cho variant này
                var allDetails = await _importRepos.QueryImportDetails()
                    .Include(d => d.Import)
                    .Where(d => d.ProductVariantId == variantId)
                    .ToListAsync();

                // 2) Loại trừ detail từ những Import gắn với Transfer
                var validDetails = new List<ImportDetail>();
                foreach (var det in allDetails)
                {
                    var isFromTransfer = await _importRepos.HasTransferForImportAsync(det.ImportId);
                    if (!isFromTransfer)
                        validDetails.Add(det);
                }

                // 3) Tính tổng số lượng
                var totalQty = validDetails.Sum(d => d.Quantity);
                if (totalQty == 0)
                    continue;   // không có data để cập nhật

                // 4) Tính tổng giá vốn
                var totalCost = validDetails.Sum(d => (d.CostPrice ?? 0m) * d.Quantity);
                var avgCost = totalCost / totalQty;

                // 5) Cộng thêm 30% lợi nhuận
                var profitRate = 0.30m;
                var avgCostWithProfit = avgCost * (1 + profitRate);

                // 6) Làm tròn đến hàng đơn vị (0 chữ số sau dấu thập phân)
                //    MidpointRounding.AwayFromZero để .5 trở lên sẽ làm tròn lên
                var finalPrice = Math.Round(avgCostWithProfit, 0, MidpointRounding.AwayFromZero);

                // 7) Cập nhật vào bảng ProductVariant
                var variant = await _importRepos.GetProductVariantByIdAsync(variantId);
                variant.Price = finalPrice;

                // 8) Tạo AuditLog cho thay đổi giá
                var log = new AuditLog
                {
                    TableName = "ProductVariant",
                    RecordId = variantId.ToString(),
                    Operation = "UPDATE",
                    ChangeDate = DateTime.Now,
                    ChangedBy = accountId,
                    ChangeData = $"{{ \"OldPrice\": ..., \"NewPrice\": {avgCost} }}",
                    Comment = "Cập nhật giá trung bình sau khi hoàn thành nhập"
                };
                _auditLogRepos.Add(log);
            }
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