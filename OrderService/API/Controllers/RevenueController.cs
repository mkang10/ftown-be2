using Application.DTO.Response;
using Application.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RevenueController : ControllerBase
    {
        private readonly RevenueHandler _revenueHandler;

        public RevenueController(RevenueHandler handler)
        {
            _revenueHandler = handler;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<ResponseDTO<RevenueSummaryResponse>>> GetRevenueSummary(
                            [FromQuery] DateTime? from,
                            [FromQuery] DateTime? to)
        {
            var result = await _revenueHandler.GetRevenueSummaryAsync(from, to);

            // Kết quả đã được wrap sẵn trong ResponseDTO => trả nguyên dạng
            return Ok(result);
        }
        [HttpGet("orders")]
        public async Task<ActionResult<ResponseDTO<List<OrderSummaryItem>>>> GetRevenueOrders(
                            [FromQuery] DateTime? from,
                            [FromQuery] DateTime? to)
        {
            var result = await _revenueHandler.GetRevenueOrdersAsync(from, to);
            return Ok(result);
        }
        [HttpGet("top-selling-products")]
        public async Task<ActionResult<ResponseDTO<List<TopSellingProductResponse>>>> GetTopSellingProducts(
                            [FromQuery] DateTime? from,
                            [FromQuery] DateTime? to,
                            [FromQuery] int top = 10)
        {
            var result = await _revenueHandler.GetTopSellingProductsAsync(from, to, top);
            return Ok(result);
        }
        [HttpGet("by-date")]
        public async Task<ActionResult<ResponseDTO<List<RevenueByDateResponse>>>> GetRevenueByDate(
                            [FromQuery] DateTime from,
                            [FromQuery] DateTime to,
                            [FromQuery] string groupBy = "day")
        {
            var result = await _revenueHandler.GetRevenueByDateAsync(from, to, groupBy);
            return Ok(result);
        }
        [HttpGet("product/{productId}/revenue")]
        public async Task<ActionResult<ResponseDTO<RevenueByProductResponse>>> GetRevenueByProduct(
                            int productId,
                            [FromQuery] DateTime? from,
                            [FromQuery] DateTime? to)
        {
            var result = await _revenueHandler.GetRevenueByProductAsync(productId, from, to);
            return Ok(result);
        }
    }

}
