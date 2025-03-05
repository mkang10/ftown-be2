using Application.DTO.Request;
using Application.DTO.Response;
using Application.DTO.Response.Domain.DTO.Response;
using Application.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryImportController : ControllerBase
    {
        private readonly ApproveHandler _appHandler;
        private readonly RejectHandler _reHandler;
        private readonly GetAllPendingHandler _getHandler;



        public InventoryImportController(ApproveHandler appHandler, RejectHandler reHandler, GetAllPendingHandler getHandler)
        {
            _appHandler = appHandler;
            _reHandler = reHandler;
            _getHandler = getHandler;
        }

        // Endpoint approve: POST api/InventoryImport/{importId}/approve
        [HttpPost("{importId}/approve")]
        public async Task<IActionResult> ApproveImport(int importId, [FromBody] ApproveRejectRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors)
                                                                  .Select(e => e.ErrorMessage));
                var response = new ResponseDTO<string>(null, false, $"Validation errors: {errors}");
                return BadRequest(response);
            }

            try
            {
                await _appHandler.ApproveImportAsync(importId, request.ChangedBy, request.Comments);
                var response = new ResponseDTO<string>("", true, "Inventory import approved successfully.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ResponseDTO<string>(null, false, $"Error: {ex.Message}");
                return StatusCode(500, errorResponse);
            }
        }

        // Endpoint reject: POST api/InventoryImport/{importId}/reject
        [HttpPost("{importId}/reject")]
        public async Task<IActionResult> RejectImport(int importId, [FromBody] ApproveRejectRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors)
                                                                  .Select(e => e.ErrorMessage));
                var response = new ResponseDTO<string>(null, false, $"Validation errors: {errors}");
                return BadRequest(response);
            }

            try
            {
                await _reHandler.RejectImportAsync(importId, request.ChangedBy, request.Comments);
                var response = new ResponseDTO<string>("", true, "Inventory import rejected successfully.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ResponseDTO<string>(null, false, $"Error: {ex.Message}");
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingInventoryImports()
        {
            try
            {
                var pendingImports = await _getHandler.GetAllPendingInventoryImportsAsync();
                var response = new ResponseDTO<List<InventoryPendingResponseDto>>(pendingImports, true, "Lấy danh sách Inventory Import pending thành công.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ResponseDTO<string>(null, false, $"Có lỗi xảy ra: {ex.Message}");
                return StatusCode(500, errorResponse);
            }
        }
    }
}
