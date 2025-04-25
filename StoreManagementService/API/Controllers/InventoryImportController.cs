using Application.DTO.Request;
using Application.UseCases;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.DTO.Response.Domain.DTO.Response;
using Domain.Entities;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryImportController : ControllerBase
    {
        private readonly CreateImportHandler _createhandler;
        private readonly GetImportHandler _gethandler;
        private readonly GetAllProductHandler _getProductVar;
        private readonly GetAllStaffHandler _getAllStaff;
        private readonly AssignStaffHandler _assignStaff;
        private readonly ImportDoneHandler _importDoneHandler;
        private readonly ImportIncompletedHandler _importIncompletedHandler;
        private readonly ImportShortageHandler _importShortageHandler;

        private readonly GetAllStaffImportHandler _getAllStaffImport;
        private readonly GetAllImportStoreHandler _getAllImportStore;





        public InventoryImportController(GetAllImportStoreHandler getAllImportStore, ImportShortageHandler importShortageHandler, GetAllStaffImportHandler getAllStaffImport, ImportDoneHandler importDoneHandler, ImportIncompletedHandler importIncompletedHandler, AssignStaffHandler assignStaff, GetAllStaffHandler getAllStaff, CreateImportHandler createhandler, GetImportHandler Gethandler, GetAllProductHandler getProductVar)
        {
            _createhandler = createhandler;
            _gethandler = Gethandler;
            _getProductVar = getProductVar;
            _getAllStaff = getAllStaff;
            _assignStaff = assignStaff;
            _importDoneHandler = importDoneHandler;
            _importIncompletedHandler = importIncompletedHandler;
            _importShortageHandler = importShortageHandler;
            _getAllStaffImport = getAllStaffImport;
            _getAllImportStore = getAllImportStore;
        }



        [HttpGet("product")]
        public async Task<IActionResult> GetAllProductVariants([FromQuery] int page = 1, [FromQuery] int pageSize = 10, string search = null)
        {
            try
            {
                var pagedVariants = await _getProductVar.GetAllProductVariantsAsync(page, pageSize,   search );
                var response = new ResponseDTO<PaginatedResponseDTO<ProductVariantResponseDto>>(pagedVariants, true, "Lấy danh sách Product Variant thành công.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ResponseDTO<string>(null, false, $"Có lỗi xảy ra: {ex.Message}");
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("names")]
        public async Task<IActionResult> GetStaffNames(int warehouseId)
        {
            var response = await _getAllStaff.GetAllStaffNamesAsync(warehouseId);
            if (!response.Status)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpPut("{importId}/assign-staff")]
        public async Task<IActionResult> AssignStaffDetail(int importId, [FromQuery] int staffDetailId)
        {
            var response = await _assignStaff.AssignStaffAccountAsync(importId, staffDetailId);
            if (!response.Status)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllImports([FromQuery] ImportFilterDto filter)
        {
            try
            {
                var response = await _gethandler.GetAllImportsAsync(filter);
                return Ok(response);
            }
            catch (KeyNotFoundException knfEx)
            {
                // Không tìm thấy dữ liệu
                var errorResponse = new ResponseDTO<object>(null, false, knfEx.Message);
                return NotFound(errorResponse);
            }
            catch (ArgumentException argEx)
            {
                // Tham số không hợp lệ
                var errorResponse = new ResponseDTO<object>(null, false, argEx.Message);
                return BadRequest(errorResponse);
            }
            catch (UnauthorizedAccessException uaEx)
            {
                // Người dùng không được phép truy cập
                var errorResponse = new ResponseDTO<object>(null, false, uaEx.Message);
                return Unauthorized(errorResponse);
            }
            catch (Exception ex)
            {
                // Các lỗi không mong đợi khác
                var errorResponse = new ResponseDTO<object>(null, false, $"Internal Server Error: {ex.Message}");
                return StatusCode(500, errorResponse);
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

                var response = await _createhandler.CreateImportAsync(request);
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

                var response = await _createhandler.CreateSupplementImportAsync(request);
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


        [HttpPost("{importId}/done")]
        public async Task<IActionResult> ProcessImportDone(int importId, int staffId ,[FromBody] List<UpdateStoreDetailDto> confirmations)
        {
            if (confirmations == null || confirmations.Count == 0)
            {
                var errorResponse = new ResponseDTO<string>(null, false, "Danh sách xác nhận không được để trống");
                return BadRequest(errorResponse);
            }

            try
            {
                await _importDoneHandler.ProcessImportDoneAsync(importId, staffId, confirmations);
                var response = new ResponseDTO<string>("Cập nhật Done thành công", true, "Success");
                return Ok(response);
            }
            catch (ArgumentException argEx)
            {
                var errorResponse = new ResponseDTO<string>(null, false, argEx.Message);
                return BadRequest(errorResponse);
            }
            catch (InvalidOperationException invOpEx)
            {
                var errorResponse = new ResponseDTO<string>(null, false, invOpEx.Message);
                return Conflict(errorResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new ResponseDTO<string>(null, false, "Đã có lỗi xảy ra, vui lòng thử lại sau");
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// API endpoint xử lý cập nhật ImportStoreDetail thành "Incompleted".
        /// Chỉ cho phép khi Import có trạng thái Processing.
        /// </summary>
        [HttpPost("{importId}/incompleted")]
        public async Task<IActionResult> ProcessImportIncompleted(int importId,int staffId, [FromBody] List<UpdateStoreDetailDto> confirmations)
        {
            if (confirmations == null || confirmations.Count == 0)
            {
                var errorResponse = new ResponseDTO<string>(null, false, "Danh sách xác nhận không được để trống");
                return BadRequest(errorResponse);
            }

            try
            {
                await _importIncompletedHandler.ProcessImportIncompletedAsync(importId, staffId, confirmations);
                var response = new ResponseDTO<string>("Cập nhật Incompleted thành công", true, "Success");
                return Ok(response);
            }
            catch (ArgumentException argEx)
            {
                var errorResponse = new ResponseDTO<string>(null, false, argEx.Message);
                return BadRequest(errorResponse);
            }
            catch (InvalidOperationException invOpEx)
            {
                var errorResponse = new ResponseDTO<string>(null, false, invOpEx.Message);
                return Conflict(errorResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new ResponseDTO<string>(null, false, "Đã có lỗi xảy ra, vui lòng thử lại sau");
                return StatusCode(500, errorResponse);
            }
        }
        [HttpGet("assign-staff")]
        public async Task<IActionResult> GetImportStoreDetail([FromQuery] ImportStoreDetailFilterDtO filter)
        {
            try
            {
                var response = await _getAllImportStore.GetStoreExportByStaffDetailAsync(filter);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<object>(null, false, $"Server error: {ex.Message}"));
            }
        }

        [HttpGet("by-staff")]
        public async Task<IActionResult> GetStoreDetailsByStaffDetail([FromQuery] ImportStoreDetailFilterDto filter)
        {
            try
            {
                var response = await _getAllStaffImport.GetStoreDetailsByStaffDetailAsync(filter);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<object>(null, false, $"Server error: {ex.Message}"));
            }
        }

        [HttpPost("shortage")]
        public async Task<IActionResult> ProcessImportShortage(
            [FromQuery] int importId,
            [FromQuery] int staffId,
            [FromBody] List<UpdateStoreDetailDto> confirmations)
        {
            try
            {
                await _importShortageHandler.ImportIncompletedAsync(importId, staffId, confirmations);
                var response = new ResponseDTO<string>(
                    data: "Cập nhật tồn kho cho đơn nhập hàng thiếu thành công",
                    status: true,
                    message: "Success"
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseDTO<string>(
                    data: null,
                    status: false,
                    message: ex.Message
                );
                return BadRequest(response);
            }
        }
    }
}


