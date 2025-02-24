using Application.DTO.Request;
using Application.DTO.Response;
using Application.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly CreateOrderHandler _createOrderHandler;
        private readonly ILogger<OrderController> _logger;

        public OrderController(CreateOrderHandler createOrderHandler, ILogger<OrderController> logger)
        {
            _createOrderHandler = createOrderHandler;
            _logger = logger;
        }

        /// <summary>
        /// 📌 Tạo đơn hàng (COD hoặc PAYOS)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
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
                var result = await _createOrderHandler.Handle(request);
                if (result == null)
                {
                    _logger.LogWarning("Tạo đơn hàng thất bại cho AccountId: {AccountId}", request.AccountId);
                    return BadRequest(new { message = "Tạo đơn hàng thất bại. Vui lòng thử lại sau." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Có lỗi xảy ra khi tạo đơn hàng cho AccountId: {AccountId}", request.AccountId);
                return StatusCode(500, new { message = "Có lỗi xảy ra trong quá trình tạo đơn hàng." });
            }
        }
    }
}

