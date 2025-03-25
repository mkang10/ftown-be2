using Domain.DTO.Request;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class ImportShortageHandler
    {
        private readonly IImportRepos _importRepos;
        private readonly IAuditLogRepos _auditLogRepos;
        private readonly IWareHouseStockRepos _wareHouseStockRepos;

        public ImportShortageHandler(IWareHouseStockRepos wareHouseStockRepos, IImportRepos importRepos, IAuditLogRepos auditLogRepos)
        {
            _importRepos = importRepos;
            _auditLogRepos = auditLogRepos;
            _wareHouseStockRepos = wareHouseStockRepos;
        }

        public async Task ProcessImportIncompletedAsync(int importId, int staffId, List<UpdateStoreDetailDto> confirmations)
        {
            var import = await _importRepos.GetByIdAssignAsync(importId);
            if (import == null)
            {
                throw new Exception("Import không tồn tại");
            }

            // Kiểm tra trạng thái Import phải là "Processing"
            if (!string.Equals(import.Status, "Processing", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Chỉ cho phép chỉnh sửa các Import có trạng thái Processing");
            }

            // Duyệt qua từng ImportDetail và cập nhật trạng thái của ImportStoreDetail cho trường hợp thiếu hàng
            foreach (var importDetail in import.ImportDetails)
            {
                foreach (var storeDetail in importDetail.ImportStoreDetails)
                {
                    var confirmation = confirmations.FirstOrDefault(c => c.ImportStoreDetailId == storeDetail.ImportStoreId);
                    if (confirmation == null)
                    {
                        continue;
                    }

                    // Cập nhật trạng thái và comment cho trường hợp hàng thiếu
                    storeDetail.Status = "Shortage";
                    storeDetail.Comments = string.IsNullOrEmpty(confirmation.Comment)
                                           ? "Hàng không đủ"
                                           : confirmation.Comment;

                    // Cập nhật số lượng thực nhận từ thông tin confirmation
                    storeDetail.ActualReceivedQuantity = confirmation.ActualReceivedQuantity;

                    // Tạo AuditLog cho mỗi ImportStoreDetail được cập nhật (nếu cần)
                    var auditLogDetail = new AuditLog
                    {
                        TableName = "ImportStoreDetail",
                        RecordId = storeDetail.ImportStoreId.ToString(),
                        Operation = "UPDATE",
                        ChangeDate = DateTime.Now,
                        ChangedBy = staffId,
                        ChangeData = $"Status updated to Shortage and ActualReceivedQuantity set to {storeDetail.ActualReceivedQuantity}",
                        Comment = storeDetail.Comments
                    };
                    _auditLogRepos.Add(auditLogDetail);
                }
            }

            // Lưu các thay đổi cập nhật trạng thái vào Import và ImportStoreDetails
            await _importRepos.SaveChangesAsync();

            // Cập nhật trạng thái của Import thành "Partial Success" nếu có bất kỳ chi tiết nào có status "Shortage"
            if (import.ImportDetails.Any(id => id.ImportStoreDetails.Any(sd => string.Equals(sd.Status, "Shortage", StringComparison.OrdinalIgnoreCase))))
            {
                import.Status = "Partial Success";
                import.CompletedDate = DateTime.Now;

                var auditLogImport = new AuditLog
                {
                    TableName = "Import",
                    RecordId = import.ImportId.ToString(),
                    Operation = "UPDATE",
                    ChangeDate = DateTime.Now,
                    ChangedBy = staffId,
                    ChangeData = "Status updated to Partial Success",
                    Comment = "Some ImportStoreDetails have status Shortage"
                };
                _auditLogRepos.Add(auditLogImport);
                // Lưu lại cập nhật import status
                await _importRepos.SaveChangesAsync();
            }

            // Gọi repository để cập nhật tồn kho dựa trên số lượng thực nhận
            await _wareHouseStockRepos.UpdateWarehouseStockAsync(import, staffId);

            // Lưu các bản ghi audit (nếu có thêm thay đổi sau khi cập nhật tồn kho)
            await _auditLogRepos.SaveChangesAsync();
        }
    }
}
