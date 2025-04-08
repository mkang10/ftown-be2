using Application.UseCases;
using Domain.DTO.Response;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseStockController : ControllerBase
    {
        private readonly GetWarehouseStockHandler _handler;

        public WarehouseStockController(GetWarehouseStockHandler handler)
        {
            _handler = handler;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _handler.HandleAsync(page, pageSize, cancellationToken);
            if (!result.Status)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
