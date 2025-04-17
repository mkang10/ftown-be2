using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Domain.DTO.Response;
using Domain.DTOs;
using Application.UseCases;
using Domain.DTO.Request;
using Domain.Entities;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseStockController : ControllerBase
    {
        private readonly GetWareHouseIdHandler _handler;
        private readonly CreateWarehouseHandler _createHandler;
        public WarehouseStockController(CreateWarehouseHandler createHandler, GetWareHouseIdHandler handler)
        {
            _createHandler = createHandler;
            _handler = handler;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDTO<WarehouseStockDto>>> Get(int id)
        {
            var dto = await _handler.GetByIdAsync(id);
            if (dto == null)
            {
                return NotFound(new ResponseDTO<WarehouseStockDto>(null!, false, "Warehouse stock not found"));
            }

            return Ok(new ResponseDTO<WarehouseStockDto>(dto, true, "Fetch warehouse stock successfully"));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ResponseDTO<int>>> Create([FromForm] WarehouseCreateDto dto)
        {
            var id = await _createHandler.HandleAsync(dto);
            return Ok(new ResponseDTO<int>(id, true, "Warehouse created successfully"));
        }

        [HttpGet("warehouse/{warehouseId}")]
        public async Task<ActionResult<PaginatedResponseDTO<GetWareHouseStockRes>>> GetByWarehouse(
            int warehouseId,
            [FromQuery] string? productName,
            [FromQuery] string? sizeName,
            [FromQuery] string? colorName,
            [FromQuery] int? stockQuantity,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _handler.GetByWarehouseIdAsync(
                warehouseId,
                productName,
                sizeName,
                colorName,
                stockQuantity,
                page,
                pageSize);
            return Ok(result);
        }
    }
}