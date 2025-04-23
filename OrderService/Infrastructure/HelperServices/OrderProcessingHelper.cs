using Application.DTO.Request;
using Application.DTO.Response;
using Application.Enums;
using Application.Interfaces;
using Application.UseCases;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.HelperServices
{
	public class OrderProcessingHelper : IOrderProcessingHelper
	{
		private readonly IPaymentRepository _paymentRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly ICustomerServiceClient _customerServiceClient;
		private readonly IMapper _mapper;
		private readonly ILogger<OrderProcessingHelper> _logger;
		private readonly AuditLogHandler _auditLogHandler;
        private readonly INotificationClient _notificationClient;
        public OrderProcessingHelper(
			IPaymentRepository paymentRepository,
			IOrderRepository orderRepository,
			ICustomerServiceClient customerServiceClient,
			IMapper mapper,
			ILogger<OrderProcessingHelper> logger,
			AuditLogHandler auditLogHandler,
            INotificationClient notificationClient)
		{
			_paymentRepository = paymentRepository;
			_orderRepository = orderRepository;
			_customerServiceClient = customerServiceClient;
			_mapper = mapper;
			_logger = logger;
			_auditLogHandler = auditLogHandler;
            _notificationClient = notificationClient;
        }

        public async Task SavePaymentAndOrderDetailsAsync(
                                                            Order order,
                                                            List<OrderDetail> orderDetails,
                                                            string paymentMethod,
                                                            decimal totalAmount,
                                                            decimal shippingCost,
                                                            long? orderCode = null
                                                        )
        {
            var payment = new Payment
            {
                OrderId = order.OrderId,
                PaymentMethod = paymentMethod,
                PaymentStatus = "Pending",
                Amount = totalAmount + shippingCost,
                TransactionDate = DateTime.UtcNow,
                OrderCode = orderCode 
            };

            await _paymentRepository.SavePaymentAsync(payment);
            await _orderRepository.SaveOrderDetailsAsync(orderDetails);
            order.OrderDetails = orderDetails;
        }

        public async Task ClearCartAsync(int accountId, List<int> productVariantIds)
		{
			var success = await _customerServiceClient.ClearCartAfterOrderAsync(accountId, productVariantIds);
			if (!success)
			{
				_logger.LogWarning("Không thể xóa sản phẩm khỏi giỏ hàng sau khi đặt hàng. AccountId: {AccountId}", accountId);
			}
		}
        public async Task LogPendingConfirmedStatusAsync(int orderId, int accountId)
        {
            await _auditLogHandler.LogOrderActionAsync(
                orderId,
                AuditOperation.CreateOrder,
                new
                {
                    InitialStatus = OrderStatus.PendingConfirmed.ToString()
                },
                accountId,
                "Đặt hàng thành công và đang đợi xác nhận ."
            );
        }
        public async Task LogPendingPaymentStatusAsync(int orderId, int accountId)
        {
            await _auditLogHandler.LogOrderActionAsync(
                orderId,
                AuditOperation.CreateOrder,
                new
                {
                    InitialStatus = OrderStatus.PendingPayment.ToString()
                },
                accountId,
                "Đặt hàng thành công và đang đợi thanh toán."
            );
        }
        public async Task LogCancelStatusAsync(int returnOrderId, int accountId)
        {
            await _auditLogHandler.LogReturnOrderActionAsync(
                returnOrderId,
                AuditOperation.CancelOrder,
                new
                {
                    InitialStatus = OrderStatus.Cancelled.ToString()
                },
                accountId,
                "Thanh toán đã bị hủy."
            );
        }
        public async Task LogPendingReturnStatusAsync(int returnOrderId, int accountId)
        {
            await _auditLogHandler.LogReturnOrderActionAsync(
                returnOrderId,
                AuditOperation.CreateReturnOrder,
                new
                {
                    InitialStatus = ReturnOrderStatus.Pending.ToString()
                },
                accountId,
                "Yêu cầu đổi/trả đã được tạo và đang chờ xử lý."
            );
        }
        public OrderResponse BuildOrderResponse(Order order, string paymentMethod, string? paymentUrl = null)
		{
			var response = _mapper.Map<OrderResponse>(order);
			response.PaymentMethod = paymentMethod;
			response.PaymentUrl = paymentUrl;
			return response;
		}
        public async Task AssignOrderToManagerAsync(int orderId, int assignedBy)
        {
            const int DefaultShopManagerId = 1;

            var assignment = new OrderAssignment
            {
                OrderId = orderId,
                ShopManagerId = DefaultShopManagerId,
                AssignmentDate = DateTime.UtcNow,
                Comments = "Tự động phân công đơn hàng khi tạo."
            };

            await _orderRepository.CreateAssignmentAsync(assignment);

            await _auditLogHandler.LogOrderActionAsync(
                orderId,
                AuditOperation.AssignToManager,
                new { ShopManagerID = DefaultShopManagerId },
                assignedBy,
                "Đơn hàng được phân công cho Shop Manager mặc định."
            );
        }
        public async Task AssignReturnOrderToManagerAsync(int orderId, int assignedBy)
        {
            const int DefaultShopManagerId = 1;

            var assignment = new OrderAssignment
            {
                OrderId = orderId,
                ShopManagerId = DefaultShopManagerId,
                AssignmentDate = DateTime.UtcNow,
                Comments = "Phân công xử lí đổi trả."
            };

            await _orderRepository.CreateAssignmentAsync(assignment);

            await _auditLogHandler.LogOrderActionAsync(
                orderId,
                AuditOperation.AssignToManager,
                new { ShopManagerID = DefaultShopManagerId },
                assignedBy,
                "Đơn hàng đổi trả được phân công cho Shop Manager mặc định."
            );
        }
        public async Task SendOrderNotificationAsync(int accountId, int orderId, string title, string message)
        {
            var notificationRequest = new SendNotificationRequest
            {
                AccountId = accountId,
                Title = title,
                Message = message,
                NotificationType = "Order",
                TargetId = orderId,
                TargetType = "Order"
            };

            await _notificationClient.SendNotificationAsync(notificationRequest);
        }

        public async Task SendReturnOrderNotificationAsync(int accountId, int returnOrderId, string title, string message)
        {
            var notificationRequest = new SendNotificationRequest
            {
                AccountId = accountId,
                Title = title,
                Message = message,
                NotificationType = "ReturnOrder",
                TargetId = returnOrderId,
                TargetType = "ReturnOrder"
            };

            await _notificationClient.SendNotificationAsync(notificationRequest);
        }
    }

}
