using Domain.DTO.Request;
using Domain.Entities;
using Domain.Interfaces;
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

        private readonly IAuditLogRepos _auditLogRepos;
        private readonly IWareHouseStockRepos _wareHouseStockRepos;

        public DispatchDoneHandler(IImportRepos importRepos, IWareHouseStockRepos wareHouseStockRepos, IDispatchRepos dispatchRepos, IAuditLogRepos auditLogRepos)
        {
            _dispatchRepos = dispatchRepos;
            _auditLogRepos = auditLogRepos;
            _wareHouseStockRepos = wareHouseStockRepos;
            _importRepos = importRepos;
        }



        public async Task ProcessDispatchDoneAsync(int dispatchId, int staffId, List<UpdateStoreDetailDto> confirmations)
        {
            // 1. Lấy Dispatch với các DispatchDetail và StoreExportStoreDetails đã được include
            var dispatch = await _dispatchRepos.GetByIdAssignAsync(dispatchId);
            if (dispatch == null)
            {
                throw new Exception("Dispatch không tồn tại");
            }

            // Kiểm tra trạng thái Dispatch: chỉ cho phép xử lý khi là "Processing" hoặc "Partial Success"
            if (!string.Equals(dispatch.Status.Trim(), "Processing", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(dispatch.Status.Trim(), "Partial Success", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Chỉ cho phép chỉnh sửa các Dispatch có trạng thái Processing hoặc Partial Success");
            }

            // 2. Cập nhật trạng thái của từng StoreExportStoreDetail dựa trên confirmations
            await UpdateDispatchStoreDetails(dispatch, confirmations, staffId);

            // Lưu thay đổi tạm thời cho StoreExportStoreDetails
            await _dispatchRepos.SaveChangesAsync();

            // Reload Dispatch để đảm bảo navigation properties được cập nhật mới nhất
            await _dispatchRepos.ReloadAsync(dispatch);

            // Lấy tất cả các StoreExportStoreDetail từ DispatchDetails
            var storeDetails = dispatch.DispatchDetails
                                       .SelectMany(dd => dd.StoreExportStoreDetails)
                                       .ToList();

            // Đếm các store detail có Status "Success" (so sánh bỏ qua khoảng trắng)
            var successDetails = storeDetails
                                 .Where(sd => !string.IsNullOrEmpty(sd.Status) &&
                                              string.Equals(sd.Status.Trim(), "Success", StringComparison.OrdinalIgnoreCase))
                                 .ToList();

            // 3. Cập nhật trạng thái Dispatch nếu tất cả StoreExportStoreDetail đều thành công
            if (storeDetails.Count > 0 && storeDetails.Count == successDetails.Count &&
                !string.Equals(dispatch.Status.Trim(), "Done", StringComparison.OrdinalIgnoreCase))
            {
                UpdateDispatchStatusToDone(dispatch, staffId);

                // Nếu Dispatch có trường OriginalDispatchId (đơn bổ sung), cập nhật đơn gốc nếu cần
                if (dispatch.OriginalId.HasValue && dispatch.OriginalId.Value > 0)
                {
                    var supplementDispatches = await _dispatchRepos.GetAllByOriginalDispatchIdAsync(dispatch.OriginalId.Value);
                    if (supplementDispatches != null && supplementDispatches.All(d => string.Equals(d.Status.Trim(), "Done", StringComparison.OrdinalIgnoreCase)))
                    {
                        var originalDispatch = await _dispatchRepos.GetByIdAsync(dispatch.OriginalId.Value);
                        if (originalDispatch != null && !string.Equals(originalDispatch.Status.Trim(), "Done", StringComparison.OrdinalIgnoreCase))
                        {
                            UpdateDispatchStatusToDone(originalDispatch, staffId);
                        }
                    }
                }

                // Cập nhật kho dựa trên toàn bộ Dispatch (từ các StoreExportStoreDetail)
                await _wareHouseStockRepos.UpdateDispatchWarehouseStockAsync(dispatch, staffId);
            }
            else
            {
                // Nếu không phải tất cả StoreExportStoreDetail đều thành công:
                // Với từng store detail có Status "Success", cập nhật kho riêng lẻ và tạo AuditLog
                if (successDetails.Count > 0)
                {
                    foreach (var detail in successDetails)
                    {
                        // Giả sử mỗi StoreExportStoreDetail nằm trong DispatchDetail và có thông tin Variant (VariantId)
                        await _wareHouseStockRepos.UpdateWarehouseStockForSingleDispatchDetailAsync(
                            detail, detail.DispatchDetail.VariantId, staffId);
                    }

                    // Nếu số lượng store detail của Dispatch là 2 và cả 2 đều thành công,
                    // có thể cập nhật Dispatch.Status thành "Success", nếu phù hợp với logic nghiệp vụ
                    if (storeDetails.Count == 2 && successDetails.Count == 2)
                    {
                        dispatch.Status = "Success";
                    }
                    else
                    {
                        dispatch.Status = "Partial Success";
                    }
                }
            }

            // Lưu các thay đổi cuối cùng cho Dispatch và AuditLog
            await _dispatchRepos.SaveChangesAsync();
            await _auditLogRepos.SaveChangesAsync();

            // 4. Cập nhật trạng thái Transfer nếu có
            // Giả sử mỗi Transfer có duy nhất một Dispatch và một Import, lấy Transfer liên quan qua dispatchId
            var transfer = await _importRepos.GetTransferByImportIdAsync(dispatch.DispatchId);
            if (transfer != null)
            {
                bool dispatchDone = transfer.Dispatch != null &&
                                    string.Equals(transfer.Dispatch.Status.Trim(), "Done", StringComparison.OrdinalIgnoreCase);
                bool importDone = transfer.Import != null &&
                                  string.Equals(transfer.Import.Status.Trim(), "Done", StringComparison.OrdinalIgnoreCase);

                if (dispatchDone && importDone && !string.Equals(transfer.Status.Trim(), "Done", StringComparison.OrdinalIgnoreCase))
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

                    await _auditLogRepos.SaveChangesAsync();
                }
            }
        }

        private async Task UpdateDispatchStoreDetails(Dispatch dispatch, List<UpdateStoreDetailDto> confirmations, int staffId)
        {
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
                        ChangedBy = staffId,
                        ChangeData = "Status updated to Success",
                        Comment = string.IsNullOrEmpty(confirmation.Comment)
                                  ? "Dispatch confirmed successfully"
                                  : confirmation.Comment
                    };
                    _auditLogRepos.Add(auditLogDetail);
                }
            }
            // Không gọi SaveChangesAsync() ở đây để tránh lỗi đồng thời trên cùng một DbContext.
        }

        private void UpdateDispatchStatusToDone(Dispatch dispatch, int staffId)
        {
            dispatch.Status = "Done";
            dispatch.CompletedDate = DateTime.Now;
            var auditLogDispatch = new AuditLog
            {
                TableName = "Dispatch",
                RecordId = dispatch.DispatchId.ToString(),
                Operation = "UPDATE",
                ChangeDate = DateTime.Now,
                ChangedBy = staffId,
                ChangeData = "Status updated to Done",
                Comment = "All StoreExportStoreDetails have status Success"
            };
            _auditLogRepos.Add(auditLogDispatch);
        }

    }
}