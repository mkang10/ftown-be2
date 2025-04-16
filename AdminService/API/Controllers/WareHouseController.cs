using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Domain.DTO.Response;
using Domain.DTOs;
using Application.UseCases;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseStockController : ControllerBase
    {
        private readonly GetWareHouseIdHandler _handler;

        public WarehouseStockController(GetWareHouseIdHandler handler)
        {
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
    }
}
