using Application.DTO.Request;
using Application.Interfaces;
using Application.UseCases;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        
        private readonly ILogger<PaymentController> _logger;
		private readonly IOrderRepository _orderRepository;
		private readonly IPaymentRepository _paymentRepository;
		private readonly IInventoryServiceClient _inventoryServiceClient;
		private readonly IOrderProcessingHelper _orderProcessingHelper;
		public PaymentController(ILogger<PaymentController> logger,
								 IOrderRepository orderRepository,
								 IPaymentRepository paymentRepository,
								 IInventoryServiceClient inventoryServiceClient,
								 IOrderProcessingHelper orderProcessingHelper)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
			_inventoryServiceClient = inventoryServiceClient;
			_orderProcessingHelper = orderProcessingHelper;
        }

		[HttpPost("webhook")]
		public async Task<IActionResult> PayOSWebhook([FromBody] PayOSCallbackRoot callbackData)
		{
			// 1. Log để xem payload PayOS gửi
			_logger.LogInformation("Nhận callback từ PayOS: {@CallbackData}", callbackData);

			// 2. Kiểm tra tính hợp lệ của callback (chữ ký, token, ...)
			if (!IsValidSignature(callbackData))
			{
				_logger.LogWarning("Callback từ PayOS không hợp lệ (chữ ký sai).");
				return BadRequest();
			}

			// 3. Kiểm tra code hoặc status để xác định giao dịch có thành công không
			// - "code": "00" => thành công
			// - "success": true => thành công
			// - "status" (trong callbackData.data) = "success" => thành công
			if (callbackData.code == "00" && callbackData.success &&
				callbackData.data != null &&
				callbackData.data.desc == "success")
			{
				// 4. Lấy mã đơn hàng từ callback
				int orderId = callbackData.data.orderCode;

				// 5. Tìm Payment/Order tương ứng trong DB
				var payment = await _paymentRepository.GetPaymentByOrderIdAsync(orderId);
				if (payment == null)
				{
					_logger.LogError("Không tìm thấy Payment cho OrderId: {OrderId}", orderId);
					return NotFound();
				}

				// 6. Cập nhật trạng thái Payment và Order
				payment.PaymentStatus = "Paid";
				await _paymentRepository.UpdatePaymentAsync(payment);

				await _orderRepository.UpdateOrderStatusAsync(orderId, "Paid");
				_logger.LogInformation("Cập nhật trạng thái đơn hàng {OrderId} thành Paid thành công.", orderId);
				payment.PaymentStatus = "Paid";
				await _paymentRepository.UpdatePaymentAsync(payment);
				await _orderRepository.UpdateOrderStatusAsync(orderId, "Paid");

				var order = await _orderRepository.GetOrderByIdAsync(orderId);
				if (order == null)
				{
					_logger.LogError("Không tìm thấy Order: {OrderId}", orderId);
					return NotFound();
				}

				var orderDetails = order.OrderDetails.ToList();

				var updateStockSuccess = await _inventoryServiceClient.UpdateStockAfterOrderAsync((int)order.WareHouseId, orderDetails);
				if (!updateStockSuccess)
				{
					_logger.LogError("Cập nhật tồn kho thất bại cho OrderId: {OrderId}", orderId);
					return StatusCode(500, "Lỗi cập nhật tồn kho.");
				}
				await _orderProcessingHelper.LogPendingConfirmedStatusAsync(orderId, order.AccountId);

			}
			else
			{
				// Trường hợp giao dịch không thành công hoặc status khác
				_logger.LogWarning("Giao dịch không thành công hoặc status không phải success.");
			}

			// 7. Trả về 200 OK để PayOS biết bạn đã nhận callback
			return Ok();
		}

		private bool IsValidSignature(PayOSCallbackRoot callbackData)
		{
			// TODO: Triển khai logic kiểm tra chữ ký (signature) với secret/key 
			// mà PayOS cung cấp cho bạn
			return true;
		}
	}
}
