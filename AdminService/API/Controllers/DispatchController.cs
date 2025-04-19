using Application.DTO.Request;
using Application.DTO.Response;
using Application.Enum;
using Application.Interfaces;
using Application.UseCases;
using Domain.DTO.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DispatchController : ControllerBase
    {
        private readonly DispatchHandler _service;

        public DispatchController(DispatchHandler service)
        {
            _service = service;
        }

        [HttpGet("dispatch/{id}")]
        public async Task<IActionResult> GetDispatchByIdController(int id)
        {
            try
            {
                var result = await _service.GetJSONDispatchByIdHandler(id);
                if (result == null)
                {
                    var notFoundResponse = new MessageRespondDTO<DispatchGet>(null, false, "Dispatch not found!");
                    return NotFound(notFoundResponse);
                }
                var successResponse = new MessageRespondDTO<DispatchGet>(result, true, "Success!");
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new MessageRespondDTO<object>(null, false, "An error occurred: " + ex.Message);
                return BadRequest(errorResponse);
            }
        }

        [HttpGet("export-store/{id}")]
        public async Task<IActionResult> GetJSONStoreExportStoreDetailByIdController(int id)
        {
            try
            {
                var result = await _service.GetJSONStoreExportStoreDetailByIdHandler(id);
                if (result == null)
                {
                    var notFoundResponse = new MessageRespondDTO<JSONStoreExportStoreDetailByIdHandlerDTO>(null, false, "Dispatch not found!");
                    return NotFound(notFoundResponse);
                }
                var successResponse = new MessageRespondDTO<JSONStoreExportStoreDetailByIdHandlerDTO>(result, true, "Success!");
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new MessageRespondDTO<object>(null, false, "An error occurred: " + ex.Message);
                return BadRequest(errorResponse);
            }
        }
    }
}
