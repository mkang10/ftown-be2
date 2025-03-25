using Domain.DTO.Request;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
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

            if (!string.Equals(import.Status, "Processing", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Chỉ cho phép chỉnh sửa các Import có trạng thái Processing");
            }

            // 1. Cập nhật trạng thái của từng ImportStoreDetail theo confirmations
            UpdateImportStoreDetails(import, confirmations, staffId);

            // 2. Kiểm tra nếu tất cả các ImportStoreDetail đều có status "Success"
            if (AllDetailsAreSuccess(import) && !string.Equals(import.Status, "Done", StringComparison.OrdinalIgnoreCase))
            {
                // 3. Cập nhật trạng thái Import thành "Done" và thêm AuditLog cho Import
                UpdateImportStatusToDone(import, staffId);

                // 4. Cập nhật Warehouse Stock và tạo WareHouseStockAudit dựa trên ImportDetail và ImportStoreDetail
                await _wareHouseStockRepos.UpdateWarehouseStockAsync(import, staffId);
            }

            // Lưu các thay đổi: Import, WarehouseStock và AuditLog
            await _importRepos.SaveChangesAsync();
            await _auditLogRepos.SaveChangesAsync();
        }

        private void UpdateImportStoreDetails(Import import, List<UpdateStoreDetailDto> confirmations, int staffId)
        {
            foreach (var importDetail in import.ImportDetails)
            {
                foreach (var storeDetail in importDetail.ImportStoreDetails)
                {
                    // Tìm confirmation dựa trên ImportStoreDetailId
                    var confirmation = confirmations.FirstOrDefault(c => c.ImportStoreDetailId == storeDetail.ImportStoreId);
                    if (confirmation == null)
                    {
                        continue;
                    }

                    // Cập nhật status thành "Success" và comment
                    storeDetail.Status = "Success";
                    storeDetail.Comments = confirmation.Comment;
                    storeDetail.ActualReceivedQuantity = storeDetail.AllocatedQuantity;

                    // Tạo AuditLog cho ImportStoreDetail
                    var auditLogDetail = new AuditLog
                    {
                        TableName = "ImportStoreDetail",
                        RecordId = storeDetail.ImportStoreId.ToString(),
                        Operation = "UPDATE",
                        ChangeDate = DateTime.Now,
                        ChangedBy = staffId,
                        ChangeData = "Status updated to Success",
                        Comment = string.IsNullOrEmpty(confirmation.Comment)
                                  ? "Stock confirmed sufficient"
                                  : confirmation.Comment
                    };
                    _auditLogRepos.Add(auditLogDetail);
                }
            }
        }

        private bool AllDetailsAreSuccess(Import import)
        {
            // Kiểm tra xem tất cả các ImportStoreDetails trong mỗi ImportDetail có status "Success"
            return import.ImportDetails.All(detail => detail.ImportStoreDetails.All(sd => sd.Status == "Success"));
        }

        private void UpdateImportStatusToDone(Import import, int staffId)
        {
            import.Status = "Done";
            import.CompletedDate = DateTime.Now;
            var auditLogImport = new AuditLog
            {
                TableName = "Import",
                RecordId = import.ImportId.ToString(),
                Operation = "UPDATE",
                ChangeDate = DateTime.Now,
                ChangedBy = staffId,
                ChangeData = "Status updated to Done",
                Comment = "All ImportStoreDetails have status Success"
            };
            _auditLogRepos.Add(auditLogImport);
        }

        
        

    }

}