using Application.UseCases;
using Domain.DTO.Response.Domain.DTO.Response;
using Domain.DTO.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Domain.DTO.Request;

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
        public async Task<ActionResult<PaginatedResponseDTO<OrderAssignmentDto>>> GetAll(
       [FromQuery] OrderAssignmentFilterDto filter,
       [FromQuery] int page = 1,
       [FromQuery] int pageSize = 10)
        {
            var result = await _getOrderHandler.GetAllAsync(filter, page, pageSize);
            return Ok(result);
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

        [HttpGet("{assignmentId}")]
        public async Task<ActionResult<OrderAssignmentDto>> GetById(int assignmentId)
        {
            var dto = await _getOrderHandler.GetByIdAsync(assignmentId);
            if (dto == null)
                return NotFound(new { Message = $"OrderAssignment {assignmentId} không tồn tại." });

            return Ok(dto);
        }
    }
}
