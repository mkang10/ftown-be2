using Application.DTO.Request;
using Application.DTO.Response;
using Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly CreateOrderHandler _createOrderHandler;
        private readonly ILogger<OrderController> _logger;
        private readonly GetOrderHistoryHandler _getOrderHistoryHandler;
        private readonly GetOrdersByStatusHandler _getOrdersByStatusHandler;
        public OrderController(CreateOrderHandler createOrderHandler, ILogger<OrderController> logger, GetOrderHistoryHandler getOrderHistoryHandler, GetOrdersByStatusHandler getOrdersByStatusHandler)
        {
            _createOrderHandler = createOrderHandler;
            _logger = logger;
            _getOrderHistoryHandler = getOrderHistoryHandler;
            _getOrdersByStatusHandler = getOrdersByStatusHandler;
        }

        /// <summary>
        /// 📌 Tạo đơn hàng (COD hoặc PAYOS)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ResponseDTO<OrderResponse>>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (!ModelState.IsValid)
            {
                // Lấy các lỗi từ ModelState
                var errors = ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
                // Trả về ResponseDTO với trạng thái false và thông báo lỗi
                return BadRequest(new ResponseDTO<OrderResponse>(null, false, "Dữ liệu không hợp lệ."));
            }

            try
            {
                var result = await _createOrderHandler.Handle(request);
                if (result == null)
                {
                    _logger.LogWarning("Tạo đơn hàng thất bại cho AccountId: {AccountId}", request.AccountId);
                    return BadRequest(new ResponseDTO<OrderResponse>(null, false, "Tạo đơn hàng thất bại. Vui lòng thử lại sau."));
                }

                return Ok(new ResponseDTO<OrderResponse>(result, true, "Tạo đơn hàng thành công."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Có lỗi xảy ra khi tạo đơn hàng cho AccountId: {AccountId}", request.AccountId);
                return StatusCode(500, new ResponseDTO<OrderResponse>(null, false, "Có lỗi xảy ra trong quá trình tạo đơn hàng."));
            }
        }
        [HttpGet("{orderId}/history")]
        public async Task<ActionResult<ResponseDTO<List<OrderHistoryResponse>>>> GetOrderHistory(int orderId)
        {
            var history = await _getOrderHistoryHandler.HandleAsync(orderId);
            return Ok(new ResponseDTO<List<OrderHistoryResponse>>(history, true, "Lịch sử đơn hàng được lấy thành công."));
        }
        [HttpGet]
        public async Task<ActionResult<ResponseDTO<List<OrderResponse>>>> GetOrdersByStatus([FromQuery] string status)
        {
            var orders = await _getOrdersByStatusHandler.HandleAsync(status);
            return Ok(new ResponseDTO<List<OrderResponse>>(orders, true, $"Danh sách đơn hàng với trạng thái {status} được lấy thành công."));
        }

    }
}
