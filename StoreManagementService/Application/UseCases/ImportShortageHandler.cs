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

        public async Task ImportIncompletedAsync(int importId, int staffId, List<UpdateStoreDetailDto> confirmations)
        {
            var import = await _importRepos.GetByIdAssignAsync(importId);
            if (import == null)
            {
                throw new Exception("Import không tồn tại");
            }

            // Chỉ cho phép xử lý các Import có trạng thái Processing hoặc Partial Success
            if (!string.Equals(import.Status, "Processing", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(import.Status, "Partial Success", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Chỉ cho phép chỉnh sửa các Import có trạng thái Processing hoặc Partial Success");
            }

            // Danh sách các store detail được cập nhật để dùng cho việc cập nhật tồn kho sau này
            var updatedStoreDetails = new List<(ImportStoreDetail storeDetail, int productVariantId)>();

            // Duyệt qua từng ImportDetail và ImportStoreDetail
            foreach (var importDetail in import.ImportDetails)
            {
                foreach (var storeDetail in importDetail.ImportStoreDetails)
                {
                    // Tìm thông tin xác nhận tương ứng
                    var confirmation = confirmations.FirstOrDefault(c => c.StoreDetailId == storeDetail.ImportStoreId);
                    if (confirmation == null)
                    {
                        continue;
                    }

                    // Kiểm tra số lượng thực nhận không vượt quá số lượng được phân bổ
                    if (confirmation.ActualReceivedQuantity > storeDetail.AllocatedQuantity)
                    {
                        throw new InvalidOperationException("ActualReceivedQuantity không được lớn hơn AllocatedQuantity, vui lòng nhập lại");
                    }

                    // Cập nhật ImportStoreDetail theo thông tin confirmation (chỉ những detail này mới bị ảnh hưởng)
                    storeDetail.Status = "Shortage";
                    storeDetail.Comments = string.IsNullOrEmpty(confirmation.Comment)
                                             ? "Hàng không đủ"
                                             : confirmation.Comment;
                    storeDetail.ActualReceivedQuantity = confirmation.ActualReceivedQuantity;

                    // Tạo AuditLog cho từng ImportStoreDetail được cập nhật
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

                    // Lưu lại store detail và variantId để cập nhật tồn kho sau
                    updatedStoreDetails.Add((storeDetail, importDetail.ProductVariantId));
                }
            }

            // Lưu các thay đổi cho ImportStoreDetail và audit log
            await _importRepos.SaveChangesAsync();

            // Nếu có bất kỳ ImportStoreDetail nào có trạng thái Shortage, cập nhật Import status thành Partial Success
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

                await _importRepos.SaveChangesAsync();
            }

            // Cập nhật tồn kho cho từng ImportStoreDetail đã được cập nhật bằng hàm UpdateWarehouseStockForSingleDetailAsync
            foreach (var item in updatedStoreDetails)
            {
                await _wareHouseStockRepos.UpdateWarehouseStockForSingleDetailAsync(item.storeDetail, item.productVariantId, staffId);
            }

            // Lưu lại các bản ghi audit được thêm trong quá trình cập nhật tồn kho (nếu có)
            await _auditLogRepos.SaveChangesAsync();
        }


    }
}