using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class UpdateReturnOrderStatusHandler
    {
        private readonly IReturnOrderRepository _returnOrderRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IOrderProcessingHelper _orderProcessingHelper;
        private readonly ILogger<UpdateReturnOrderStatusHandler> _logger;

        public UpdateReturnOrderStatusHandler(
            IReturnOrderRepository returnOrderRepository,
            IAuditLogRepository auditLogRepository,
            IOrderProcessingHelper orderProcessingHelper,
            ILogger<UpdateReturnOrderStatusHandler> logger)
        {
            _returnOrderRepository = returnOrderRepository;
            _auditLogRepository = auditLogRepository;
            _orderProcessingHelper = orderProcessingHelper;
            _logger = logger;
        }

        public async Task<bool> HandleAsync(int returnOrderId, string newStatus, int changedBy, string? comment)
        {
            var returnOrder = await _returnOrderRepository.GetByIdAsync(returnOrderId);
            if (returnOrder == null)
            {
                _logger.LogWarning($"[UpdateReturnOrderStatus] ReturnOrderId {returnOrderId} không tồn tại.");
                return false;
            }

            var previousStatus = returnOrder.Status;

            // Cập nhật trạng thái và thời gian cập nhật
            returnOrder.Status = newStatus;
            returnOrder.UpdatedDate = DateTime.UtcNow;
            await _returnOrderRepository.UpdateAsync(returnOrder);

            // Ghi log thay đổi
            var changeData = JsonSerializer.Serialize(new
            {
                OldStatus = previousStatus,
                NewStatus = newStatus
            });

            await _auditLogRepository.AddAuditLogAsync(
                "ReturnOrders",
                returnOrderId.ToString(),
                newStatus,
                changedBy,
                changeData,
                comment
            );

            // Gửi thông báo tái sử dụng method có sẵn
            try
            {
                var message = newStatus switch
                {
                    "Approved" => $"Yêu cầu đổi/trả đơn hàng #{returnOrder.OrderId} của bạn đã được phê duyệt.",
                    "Rejected" => $"Yêu cầu đổi/trả đơn hàng #{returnOrder.OrderId} của bạn đã bị từ chối.",
                    _ => $"Yêu cầu đổi/trả đơn hàng #{returnOrder.OrderId} đã được cập nhật trạng thái: {newStatus}."
                };

                await _orderProcessingHelper.SendReturnOrderNotificationAsync(
                    returnOrder.AccountId,
                    returnOrder.ReturnOrderId,
                    "Cập nhật trạng thái đổi/trả đơn hàng",
                    message
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"[NotifyError] Gửi thông báo thất bại: {ex.Message}");
            }

            return true;
        }
    }

}
