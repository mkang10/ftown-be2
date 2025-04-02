using Application.DTO.Response;
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
		public OrderProcessingHelper(
			IPaymentRepository paymentRepository,
			IOrderRepository orderRepository,
			ICustomerServiceClient customerServiceClient,
			IMapper mapper,
			ILogger<OrderProcessingHelper> logger,
			AuditLogHandler auditLogHandler)
		{
			_paymentRepository = paymentRepository;
			_orderRepository = orderRepository;
			_customerServiceClient = customerServiceClient;
			_mapper = mapper;
			_logger = logger;
			_auditLogHandler = auditLogHandler;
		}

		public async Task SavePaymentAndOrderDetailsAsync(Order order, List<OrderDetail> orderDetails, string paymentMethod, decimal totalAmount, decimal shippingCost)
		{
			var payment = new Payment
			{
				OrderId = order.OrderId,
				PaymentMethod = paymentMethod,
				PaymentStatus = "Pending",
				Amount = totalAmount + shippingCost,
				TransactionDate = DateTime.UtcNow
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
			await _auditLogHandler.LogOrderStatusChangeAsync(
				orderId,
				"Pending Confirmed",
				accountId,
				"Đơn hàng đang chờ xác nhận."
			);
		}

		public OrderResponse BuildOrderResponse(Order order, string paymentMethod, string? paymentUrl = null)
		{
			var response = _mapper.Map<OrderResponse>(order);
			response.PaymentMethod = paymentMethod;
			response.PaymentUrl = paymentUrl;
			return response;
		}
	}

}
