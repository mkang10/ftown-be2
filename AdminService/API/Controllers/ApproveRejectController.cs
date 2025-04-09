using Application.UseCases;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
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
        private readonly CreateImportHandler _createImportHandler;
        private readonly GetWareHouseHandler _getWareHouseHandler;



        public InventoryImportController(GetWareHouseHandler getWareHouseHandler, CreateImportHandler createImportHandler, GetImportDetailHandler getDetailHandler, ApproveHandler appHandler, RejectHandler reHandler, GetAllImportHandler getHandler)
        {
            _appHandler = appHandler;
            _reHandler = reHandler;
            _getHandler = getHandler;
            _getDetailHandler = getDetailHandler;
            _createImportHandler = createImportHandler;
            _getWareHouseHandler = getWareHouseHandler;
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

        [HttpPost("create")]
        public async Task<IActionResult> CreateImport([FromBody] CreateImportDto request)
        {
            try
            {
                if (request == null || request.ImportDetails == null || !request.ImportDetails.Any())
                {
                    return BadRequest(new ResponseDTO<object>(null, false, "Dữ liệu import không hợp lệ!"));
                }

                var response = await _createImportHandler.CreateImportAsync(request);
                return Ok(response);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new ResponseDTO<object>(null, false, argEx.Message));
            }
            catch (UnauthorizedAccessException uaEx)
            {
                return Unauthorized(new ResponseDTO<object>(null, false, uaEx.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<object>(null, false, $"Lỗi server: {ex.Message}"));
            }
        }

        [HttpPost("create-supplement")]
        public async Task<IActionResult> CreateSupplementImport([FromBody] SupplementImportRequestDto request)
        {
            try
            {
                if (request == null || request.ImportDetails == null || !request.ImportDetails.Any())
                    return BadRequest(new ResponseDTO<object>(null, false, "Dữ liệu import không hợp lệ!"));
                if (request.OriginalImportId <= 0)
                    return BadRequest(new ResponseDTO<object>(null, false, "OriginalImportId không hợp lệ!"));

                var response = await _createImportHandler.CreateSupplementImportAsync(request);
                return Ok(response);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new ResponseDTO<object>(null, false, argEx.Message));
            }
            catch (UnauthorizedAccessException uaEx)
            {
                return Unauthorized(new ResponseDTO<object>(null, false, uaEx.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<object>(null, false, $"Lỗi server: {ex.Message}"));
            }
        }

        [HttpGet("WareHouse")]
        public async Task<IActionResult> GetAllWarehouses([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var pagedWareHouse = await _getWareHouseHandler.GetAllWareHouse(page, pageSize);
                var response = new ResponseDTO<PaginatedResponseDTO<Warehouse>>(pagedWareHouse, true, "Lấy danh sách warehouse thành công.");
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
