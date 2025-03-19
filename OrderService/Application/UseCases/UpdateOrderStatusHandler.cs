using Application.DTO.Request;
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
    public class UpdateOrderStatusHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public UpdateOrderStatusHandler(IOrderRepository orderRepository, IAuditLogRepository auditLogRepository)
        {
            _orderRepository = orderRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<bool> HandleAsync(int orderId, string newStatus, int changedBy, string? comment)
        {
            // 📌 1️⃣ Lấy thông tin đơn hàng
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return false;
            }

            // 📌 2️⃣ Cập nhật trạng thái đơn hàng
            await _orderRepository.UpdateOrderStatusAsync(orderId, newStatus);

            // 📌 3️⃣ Ghi log vào AuditLog
            var previousStatus = order.Status; // Lấy trạng thái cũ

            var changeData = JsonSerializer.Serialize(new
            {
                OldStatus = previousStatus,
                NewStatus = newStatus
            });

            await _auditLogRepository.AddAuditLogAsync(
                "Orders",
                orderId.ToString(),
                newStatus,
                changedBy,
                changeData, // ✅ Lưu dữ liệu thay đổi
                comment
            );



            return true;
        }
    }

}
