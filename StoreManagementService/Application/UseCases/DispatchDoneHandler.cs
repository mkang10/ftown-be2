using Domain.DTO.Request;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class DispatchDoneHandler
    {
        private readonly IDispatchRepos _dispatchRepos;
        private readonly IImportRepos _importRepos;
        private readonly IStaffDetailRepository _staffRepos;

        private readonly IAuditLogRepos _auditLogRepos;
        private readonly IWareHouseStockRepos _wareHouseStockRepos;

        public DispatchDoneHandler(IStaffDetailRepository staffRepos, IImportRepos importRepos, IWareHouseStockRepos wareHouseStockRepos, IDispatchRepos dispatchRepos, IAuditLogRepos auditLogRepos)
        {
            _dispatchRepos = dispatchRepos;
            _auditLogRepos = auditLogRepos;
            _wareHouseStockRepos = wareHouseStockRepos;
            _importRepos = importRepos;
            _staffRepos = staffRepos;
        }



        public async Task ProcessDispatchDoneAsync(
     int dispatchId,
     int staffId,
     List<UpdateStoreDetailDto> confirmations)
        {
            // 1. Lấy Dispatch với các DispatchDetail và StoreExportStoreDetails đã include
            var dispatch = await _dispatchRepos.GetByIdAssignAsync(dispatchId);
            if (dispatch == null)
                throw new Exception("Đơn xuất hàng không tồn tại");

            // 2. Validate status
            if (!string.Equals(dispatch.Status.Trim(), "Approved", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(dispatch.Status.Trim(), "Processing", StringComparison.OrdinalIgnoreCase))

            {
                throw new InvalidOperationException(
                    "Chỉ cho phép chỉnh sửa các đơn xuất hàng có trạng thái Approved hoặc Processing");
            }

            // Danh sách các DispatchStoreDetailId vừa confirm
            var confirmedIds = confirmations.Select(c => c.StoreDetailId).ToList();

            // 3. Cập nhật trạng thái của từng StoreExportStoreDetail
            await UpdateDispatchStoreDetails(dispatch, confirmations, staffId);
            await _dispatchRepos.SaveChangesAsync();
            await _dispatchRepos.ReloadAsync(dispatch);

            // 4. Tính list tất cả storeDetail và những storeDetail thành công
            var storeDetails = dispatch.DispatchDetails
                                       .SelectMany(dd => dd.StoreExportStoreDetails)
                                       .ToList();

            var successDetails = storeDetails
                .Where(sd => !string.IsNullOrEmpty(sd.Status) &&
                             string.Equals(sd.Status.Trim(), "Success", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // 5. Nếu tất cả đều Success → mark Done, update supplement → full-batch stock update
            if (storeDetails.Count > 0 &&
                storeDetails.Count == successDetails.Count &&
                !string.Equals(dispatch.Status.Trim(), "Done", StringComparison.OrdinalIgnoreCase))
            {
                UpdateDispatchStatusToDone(dispatch, staffId);

                // Nếu là supplement, cập nhật dispatch gốc
                if (dispatch.OriginalId.HasValue && dispatch.OriginalId.Value > 0)
                {
                    var supplement = await _dispatchRepos.GetAllByOriginalDispatchIdAsync(dispatch.OriginalId.Value);
                    if (supplement != null && supplement.All(d => d.Status.Trim().Equals("Done", StringComparison.OrdinalIgnoreCase)))
                    {
                        var original = await _dispatchRepos.GetByIdAsync(dispatch.OriginalId.Value);
                        if (original != null && !original.Status.Trim().Equals("Done", StringComparison.OrdinalIgnoreCase))
                            UpdateDispatchStatusToDone(original, staffId);
                    }
                }

                // Full‐batch update: chỉ trên confirmedIds
                await _wareHouseStockRepos.UpdateDispatchWarehouseStockAsync(
                    dispatch,
                    staffId,
                    confirmedIds
                );
            }
            else
            {
                // 6. Partial‐batch: chỉ update những successDetails vừa confirm
                var toProcess = successDetails
                    .Where(sd => confirmedIds.Contains(sd.DispatchStoreDetailId))
                    .ToList();

                if (toProcess.Any())
                {
                    foreach (var detail in toProcess)
                    {
                        await _wareHouseStockRepos.UpdateWarehouseStockForSingleDispatchDetailAsync(
                            detail,
                            detail.DispatchDetail.VariantId,
                            staffId
                        );
                    }

                    // Cập nhật status tổng của Dispatch
                    // Cập nhật status tổng của Dispatch
                    if (toProcess.Count == storeDetails.Count)
                    {
                        // tất cả đều thành công
                        dispatch.Status = "Done";
                    }

                    else
                    {
                        dispatch.Status = "Processing";
                    }
                    
                }
            }

            // 7. Lưu và ghi AuditLog cho Dispatch, stock
            await _dispatchRepos.SaveChangesAsync();
            await _auditLogRepos.SaveChangesAsync();


            // 8. Cập nhật Transfer nếu có
            var transfer = await _importRepos.GetTransferByImportIdAsync(dispatch.DispatchId);
            if (transfer != null)
            {
                int accountId = await _staffRepos.GetAccountIdByStaffIdAsync(staffId);
                bool dispDone = transfer.Dispatch?.Status.Trim().Equals("Done", StringComparison.OrdinalIgnoreCase) == true;
                bool impDone = transfer.Import?.Status.Trim().Equals("Done", StringComparison.OrdinalIgnoreCase) == true;

                if (dispDone && impDone && !transfer.Status.Trim().Equals("Done", StringComparison.OrdinalIgnoreCase))
                {
                    transfer.Status = "Done";
                    _auditLogRepos.Add(new AuditLog
                    {
                        TableName = "Transfer",
                        RecordId = transfer.TransferOrderId.ToString(),
                        Operation = "UPDATE",
                        ChangeDate = DateTime.Now,
                        ChangedBy = accountId,
                        ChangeData = "Status updated to Done",
                        Comment = "Đơn xuất và nhập hàng đã hoàn thành"
                    });
                    await _auditLogRepos.SaveChangesAsync();
                }
            }
        }


        private async Task UpdateDispatchStoreDetails(Dispatch dispatch, List<UpdateStoreDetailDto> confirmations, int staffId)
        {
            var accountId = await _staffRepos.GetAccountIdByStaffIdAsync(staffId);
            if (accountId == null)
                throw new KeyNotFoundException($"Không tìm thấy Account cho StaffId={staffId}");
            foreach (var dispatchDetail in dispatch.DispatchDetails)
            {
                foreach (var storeDetail in dispatchDetail.StoreExportStoreDetails)
                {
                    // Tìm confirmation dựa trên DispatchStoreDetailId (ở đây tương ứng với DispatchStoreDetailId của StoreExportStoreDetail)
                    var confirmation = confirmations.FirstOrDefault(c => c.StoreDetailId == storeDetail.DispatchStoreDetailId);
                    if (confirmation == null)
                    {
                        continue;
                    }

                    // Cập nhật trạng thái thành "Success" và gán comment từ confirmation
                    storeDetail.Status = "Success";
                    storeDetail.Comments = confirmation.Comment;

                    // Nếu có thuộc tính cập nhật số lượng thực tế, có thể gán:
                    storeDetail.ActualQuantity = storeDetail.AllocatedQuantity;

                    // Tạo AuditLog cho StoreExportStoreDetail
                    var auditLogDetail = new AuditLog
                    {
                        TableName = "StoreExportStoreDetail",
                        RecordId = storeDetail.DispatchStoreDetailId.ToString(),
                        Operation = "UPDATE",
                        ChangeDate = DateTime.Now,
                        ChangedBy = accountId,
                        ChangeData = "Status updated to Success",
                        Comment = "Đã xuất hàng thành công !"
                    };
                    _auditLogRepos.Add(auditLogDetail);
                }
            }
            // Không gọi SaveChangesAsync() ở đây để tránh lỗi đồng thời trên cùng một DbContext.
        }

        private async Task UpdateDispatchStatusToDone(Dispatch dispatch, int staffId)
        {
            var accountId = await _staffRepos.GetAccountIdByStaffIdAsync(staffId);
            if (accountId == null)
                throw new KeyNotFoundException($"Không tìm thấy Account cho StaffId={staffId}");
            dispatch.Status = "Done";
            dispatch.CompletedDate = DateTime.Now;
            var auditLogDispatch = new AuditLog
            {
                TableName = "Dispatch",
                RecordId = dispatch.DispatchId.ToString(),
                Operation = "UPDATE",
                ChangeDate = DateTime.Now,
                ChangedBy = accountId,
                ChangeData = "Status updated to Done",
                Comment = "Tất cả đơn xuất hàng đã được xuất khỏi kho !"
            };
            _auditLogRepos.Add(auditLogDispatch);
        }

    }
}