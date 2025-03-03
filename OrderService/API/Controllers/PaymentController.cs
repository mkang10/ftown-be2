using Application.DTO.Request;
using Application.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ProcessPaymentHandler _processPaymentHandler;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(ProcessPaymentHandler processPaymentHandler, ILogger<PaymentController> logger)
        {
            _processPaymentHandler = processPaymentHandler;
            _logger = logger;
        }

        /// <summary>
        /// 📌 Xử lý thanh toán đơn hàng
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<string>> ProcessPayment([FromBody] PaymentRequest request)
        {
            // Kiểm tra validation của request
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                       .SelectMany(v => v.Errors)
                                       .Select(e => e.ErrorMessage);
                return BadRequest(new { message = "Dữ liệu không hợp lệ.", errors });
            }

            try
            {
                var paymentUrl = await _processPaymentHandler.Handle(request);
                if (string.IsNullOrEmpty(paymentUrl))
                {
                    _logger.LogWarning("Xử lý thanh toán thất bại cho OrderId: {OrderId}", request.OrderId);
                    return BadRequest(new { message = "Xử lý thanh toán thất bại. Vui lòng kiểm tra lại thông tin đơn hàng hoặc thử lại sau." });
                }

                return Ok(new { paymentUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Có lỗi xảy ra khi xử lý thanh toán cho OrderId: {OrderId}", request.OrderId);
                return StatusCode(500, new { message = "Có lỗi xảy ra trong quá trình xử lý thanh toán." });
            }
        }

        /// <summary>
        /// 📌 PAYOS Callback cập nhật trạng thái thanh toán
        /// </summary>
        [HttpPost("callback")]
        public async Task<IActionResult> PaymentCallback([FromBody] PayOSCallbackRequest callback)
        {
            // Kiểm tra validation của request
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                       .SelectMany(v => v.Errors)
                                       .Select(e => e.ErrorMessage);
                return BadRequest(new { message = "Dữ liệu callback không hợp lệ.", errors });
            }

            try
            {
                var result = await _processPaymentHandler.HandleCallback(callback);
                if (!result)
                {
                    _logger.LogWarning("Xác nhận thanh toán thất bại cho OrderId: {OrderId}", callback.OrderId);
                    return BadRequest(new { message = "Xác nhận thanh toán thất bại." });
                }

                return Ok(new { message = "Thanh toán thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Có lỗi xảy ra khi xử lý callback thanh toán cho OrderId: {OrderId}", callback.OrderId);
                return StatusCode(500, new { message = "Có lỗi xảy ra trong quá trình xác nhận thanh toán." });
            }
        }
    }
}
