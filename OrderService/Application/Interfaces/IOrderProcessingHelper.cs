using Application.DTO.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
	public interface IOrderProcessingHelper
	{
		Task SavePaymentAndOrderDetailsAsync(Order order, List<OrderDetail> orderDetails, string paymentMethod, decimal totalAmount, decimal shippingCost);
		Task ClearCartAsync(int accountId, List<int> productVariantIds);
		Task LogPendingConfirmedStatusAsync(int orderId, int accountId);
		OrderResponse BuildOrderResponse(Order order, string paymentMethod, string? paymentUrl = null);
	}
}
