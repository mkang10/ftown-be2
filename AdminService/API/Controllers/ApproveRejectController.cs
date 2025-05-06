using Application.Services;
using Application.UseCases;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        private readonly IImportRepos _importRepos;
        private readonly ReportService _reportService;



        public InventoryImportController(IImportRepos importRepos, ReportService reportService, GetWareHouseHandler getWareHouseHandler, CreateImportHandler createImportHandler, GetImportDetailHandler getDetailHandler, ApproveHandler appHandler, RejectHandler reHandler, GetAllImportHandler getHandler)
        {
            _appHandler = appHandler;
            _reHandler = reHandler;
            _getHandler = getHandler;
            _getDetailHandler = getDetailHandler;
            _createImportHandler = createImportHandler;
            _getWareHouseHandler = getWareHouseHandler;
            _importRepos = importRepos;
            _reportService = reportService;
        }


        [HttpPost("{importId}/approve")]
        public async Task<IActionResult> ApproveImport(
        int importId,
        [FromBody] ApproveRejectRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                                                    .SelectMany(v => v.Errors)
                                                    .Select(e => e.ErrorMessage));
                return BadRequest(new ResponseDTO<string>(null, false, $"Validation errors: {errors}"));
            }

            try
            {
                // 1. Thực hiện approve
                await _appHandler.ApproveImportAsync(importId, request.ChangedBy, request.Comments);

                // 2. Load lại entity Import với chi tiết để làm báo cáo
                //    (Giả sử GetByIdAsyncWithDetails bao gồm ImportDetails + ImportStoreDetails)
                var importEntity = await _importRepos.GetByIdAsyncWithDetails(importId);
                if (importEntity == null)
                    return NotFound(new ResponseDTO<string>(null, false, "Không tìm thấy đơn nhập đã approve."));

                // 3. Gọi ReportService để sinh file báo cáo Import Slip
                byte[] slipBytes = _reportService.GenerateImportSlip(importEntity);

                // 4. Trả về file để client download
                string fileName = $"PhieuNhap_{importEntity.ReferenceNumber}_{DateTime.Now:yyyyMMddHHmmss}.docx";
                return File(
                    slipBytes,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    fileName
                );
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new ResponseDTO<string>(null, false, argEx.Message));
            }
            catch (UnauthorizedAccessException uaEx)
            {
                return Unauthorized(new ResponseDTO<string>(null, false, uaEx.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<string>(null, false, $"Lỗi server: {ex.Message}"));
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
        public async Task<IActionResult> CreateImport([FromBody] PurchaseImportCreateDto request)
        {
            try
            {
                //if (request == null || request. == null || !request.ImportDetails.Any())
                //    return BadRequest(new ResponseDTO<object>(null, false, "Dữ liệu import không hợp lệ!"));

                var response = await _createImportHandler.CreatePurchaseImportAsync(request);
                if (!response.Status)
                    return BadRequest(response);

                // Sau khi tạo thành công, load lại Import từ repository (theo ImportId được trả về trong response)
                var importEntity = await _importRepos.GetByIdAsync(response.Data.ImportId);
                if (importEntity == null)
                    return NotFound(new ResponseDTO<object>(null, false, "Không tìm thấy đơn nhập sau khi tạo."));

                // Gọi ReportService để tạo file biên bản nhập hàng
                byte[] slipFile = _reportService.GenerateImportSlip(importEntity);
                string fileName = $"PhieuNhap_{importEntity.ReferenceNumber}_{DateTime.Now:yyyyMMddHHmmss}.docx";
                return File(slipFile, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
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

        [HttpPost("create-from-excel")]
 
        public async Task<IActionResult> CreateImportFromExcel(
        [FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new ResponseDTO<object>(null, false, "Vui lòng upload file Excel."));


                // Gọi service để import từ Excel
                var response = await _createImportHandler.CreatePurchaseImportFromExcelAsync(file, 18);
                if (!response.Status)
                    return BadRequest(response);

                // Lấy lại entity import vừa tạo
                var importEntity = await _importRepos.GetByIdAsync(response.Data.ImportId);
                if (importEntity == null)
                    return NotFound(new ResponseDTO<object>(null, false, "Không tìm thấy đơn nhập sau khi tạo."));

                // Sinh file biên bản nhập kho
                byte[] slipFile = _reportService.GenerateImportSlip(importEntity);
                string fileName = $"PhieuNhap_{importEntity.ReferenceNumber}_{DateTime.Now:yyyyMMddHHmmss}.docx";

                return File(
                    slipFile,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    fileName
                );
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

        /// <summary>
        /// Tạo đơn nhập hàng bổ sung và trả về file Word (biên bản nhập hàng) dựa trên đơn bổ sung vừa tạo.
        /// </summary>
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
                if (!response.Status)
                    return BadRequest(response);

                // Sau khi tạo đơn bổ sung, load lại đơn bổ sung và đơn cũ đầy đủ dữ liệu
                var supplementImportEntity = await _importRepos.GetByIdAsync(response.Data.ImportData.ImportId);
                var oldImportEntity = await _importRepos.GetByIdAsync(response.Data.ImportData.OriginalImportId.Value);
                if (supplementImportEntity == null || oldImportEntity == null)
                    return NotFound(new ResponseDTO<object>(null, false, "Không tìm thấy dữ liệu đơn nhập khi tạo báo cáo."));

                // Tạo báo cáo nhập bổ sung
                byte[] reportFileBytes = _reportService.GenerateImportSupplementSlip(supplementImportEntity, oldImportEntity);
                string fileName = $"PhieuNhapBoSung_{supplementImportEntity.ReferenceNumber}_{DateTime.Now:yyyyMMddHHmmss}.docx";
                return File(reportFileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
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
