using Application.UseCases;
using Domain.DTO.Request;
using Domain.DTO.Response;
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
        private readonly GetAllImportHandler _getHandler;
        private readonly GetImportDetailHandler _getDetailHandler;



        public InventoryImportController(GetImportDetailHandler getDetailHandler, ApproveHandler appHandler, RejectHandler reHandler, GetAllImportHandler getHandler)
        {
            _appHandler = appHandler;
            _reHandler = reHandler;
            _getHandler = getHandler;
            _getDetailHandler = getDetailHandler;
        }
      

        //// Endpoint approve: POST api/InventoryImport/{importId}/approve
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

        [HttpGet("filter")]
        public async Task<IActionResult> GetInventoryImports([FromQuery] InventoryImportFilterDto filter)
        {
            try
            {
                var pagedResult = await _getHandler.GetInventoryImportsAsync(filter);
                var response = new ResponseDTO<PagedResult<InventoryImportResponseDto>>(
                    pagedResult,
                    true,
                    "Lấy danh sách Inventory Import thành công."
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ResponseDTO<string>(null, false, $"Có lỗi xảy ra: {ex.Message}");
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("{importId}")]
        public async Task<IActionResult> GetInventoryDetail(int importId)
        {
            try
            {
                var inventoryDetail = await _getDetailHandler.GetInventoryDetailAsync(importId);
                var response = new ResponseDTO<InventoryImportDetailDto>(inventoryDetail, true, "Lấy dữ liệu thành công.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseDTO<InventoryImportDetailDto>(null, false, ex.Message);
                return BadRequest(response);
            }
        }
    }
}
