using Application.DTO.Request;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class UpdateOrderStatusHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateOrderStatusHandler> _logger;

        public UpdateOrderStatusHandler(
            IOrderRepository orderRepository,
            IPaymentRepository paymentRepository,
            IUnitOfWork unitOfWork,
            ILogger<UpdateOrderStatusHandler> logger)
        {
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> Handle(UpdateOrderStatusRequest request)
        {
            try
            {
                _logger.LogInformation($"Processing Order {request.OrderId} with PaymentStatus: {request.PaymentStatus}");

                // Lấy đơn hàng từ repository
                var order = await _orderRepository.GetOrderByIdAsync(request.OrderId);
                if (order == null)
                {
                    _logger.LogWarning($"Order {request.OrderId} not found.");
                    return false;
                }

                // Lấy thông tin thanh toán
                var payment = await _paymentRepository.GetPaymentByOrderIdAsync(request.OrderId);
                if (payment == null)
                {
                    _logger.LogWarning($"Payment record for Order {request.OrderId} not found.");
                    return false;
                }

                // Cập nhật trạng thái thanh toán
                payment.PaymentStatus = request.PaymentStatus;
                await _paymentRepository.UpdatePaymentAsync(payment);

                // Nếu thanh toán thành công, cập nhật trạng thái đơn hàng
                if (request.PaymentStatus == "Success")
                {
                    order.Status = "Confirmed"; // Đơn hàng được xác nhận sau khi thanh toán
                }
                else if (request.PaymentStatus == "Failed")
                {
                    order.Status = "Payment Failed"; // Đơn hàng thất bại do lỗi thanh toán
                }

                await _orderRepository.UpdateOrderAsync(order);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Order {request.OrderId} updated successfully.");
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, $"Failed to update Order {request.OrderId} status.");
                return false;
            }
        }
    }
}
