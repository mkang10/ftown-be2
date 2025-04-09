using Application.UseCases;
using Domain.DTO.Response.Domain.DTO.Response;
using Domain.DTO.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Domain.DTO.Request;
using static Domain.DTO.Response.OrderDTO;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly GetOrderHandler _getOrderHandler;
        private readonly CompletedOrderHandler _completedOrder;

        private readonly AssignStaffHandler _assign;

        public OrdersController(CompletedOrderHandler completedOrder ,AssignStaffHandler assign, GetOrderHandler getOrderHandler)
        {
            _getOrderHandler = getOrderHandler;
            _assign = assign;
            _completedOrder = completedOrder;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrdersWithAssignments(
              [FromQuery] int page = 1,
              [FromQuery] int pageSize = 10,
              [FromQuery] string? orderStatus = null,
              [FromQuery] DateTime? orderStartDate = null,
              [FromQuery] DateTime? orderEndDate = null,
              [FromQuery] int? shopManagerId = null,
              [FromQuery] int? staffId = null,
              [FromQuery] DateTime? assignmentStartDate = null,
              [FromQuery] DateTime? assignmentEndDate = null)
        {
            var response = await _getOrderHandler.GetAllOrdersWithAssignmentsAsync(
                page,
                pageSize,
                orderStatus,
                orderStartDate,
                orderEndDate,
                shopManagerId,
                staffId,
                assignmentStartDate,
                assignmentEndDate);

            if (response.Status)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }

        [HttpPut("assign")]
        public async Task<IActionResult> AssignStaff([FromBody] AssignStaffDTO dto)
        {
            var result = await _assign.AssignStaffAsync(dto);
            return result.Status ? Ok(result) : NotFound(result);
        }

        [HttpPut("{orderId}/complete")]
        public async Task<IActionResult> CompleteOrder(int orderId)
        {
            var result = await _completedOrder.CompleteOrderAsync(orderId);
            if (!result.Status)
                return NotFound(result);

            return Ok(result);
        }
    }
}
