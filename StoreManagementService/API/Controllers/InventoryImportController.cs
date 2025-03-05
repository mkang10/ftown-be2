using Application.DTO.Request;
using Application.UseCases;
using Domain.DTO.Response;
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
        private readonly InventoryImportHandler _handler;
        private readonly GetInventoryImportHandler _Gethandler;


        public InventoryImportController(InventoryImportHandler handler, GetInventoryImportHandler Gethandler)
        {
            _handler = handler;
            _Gethandler = Gethandler;
        }

        [HttpPost]
        public async Task<IActionResult> CreateInventoryImport([FromBody] CreateInventoryImportDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors)
                                                                  .Select(e => e.ErrorMessage));
                var badResponse = new ResponseDTO<string>(null, false, $"Validation errors: {errors}");
                return BadRequest(badResponse);
            }

            try
            {
                var importId = await _handler.CreateInventoryImportAsync(dto);

                if (importId <= 0)
                {
                    var response = new ResponseDTO<string>(null, false, "Không thể tạo Inventory Import. Vui lòng kiểm tra dữ liệu đầu vào.");
                    return BadRequest(response);
                }

                var successResponse = new ResponseDTO<int>(importId, true, "Inventory Import được tạo thành công.");
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new ResponseDTO<string>(null, false, $"Có lỗi xảy ra: {ex.Message}");
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("byUser")]
        public async Task<IActionResult> GetInventoryImportsByCreatedBy([FromQuery] int createdBy)
        {
            try
            {
                var imports = await _Gethandler.GetAllInventoryImportsByCreatedByAsync(createdBy);
                var response = new ResponseDTO<List<InventoryImportResponseDto>>(imports, true, "Lấy danh sách Inventory Import thành công.");
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
