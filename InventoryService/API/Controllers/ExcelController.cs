using Application.DTO.Response;
using Application.Enum;
using Application.Interfaces;
using Domain.Commons;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExcelController : ControllerBase
    {
        private readonly IExcelService _service;
        private readonly IExcelRepo _repo;

        public ExcelController(IExcelService service, IExcelRepo repo)
        {
            _service = service;
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParameter paginationParameter)
        {
            try
            {
                var result = await _service.GetAllProduct(paginationParameter);

                if (result == null)
                {
                    var notFoundResponse = new MessageRespondDTO<object>(null, false, StatusSuccess.Wrong.ToString());
                    return NotFound(notFoundResponse);
                }
                else
                {
                    var metadata = new
                    {
                        result.TotalCount,
                        result.PageSize,
                        result.CurrentPage,
                        result.TotalPages,
                        result.HasNext,
                        result.HasPrevious
                    };

                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                }
                var successResponse = new MessageRespondDTO<object>(result, true, StatusSuccess.Success.ToString());
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new MessageRespondDTO<object>(null, false, "An error occurred: " + ex.Message);
                return BadRequest(errorResponse);
            }
        }

        [HttpGet("export-excel")]
        public async Task<IActionResult> Export([FromQuery] PaginationParameter paginationParameter)
        {
            try
            {
                var result = await _repo.GetAllProduct(paginationParameter);

                // LicenseContext
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // create excel
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Products");

                    // Dat ten cho chung
                    worksheet.Cells[1, 1].Value = "ProductId";
                    worksheet.Cells[1, 2].Value = "Name";
                    worksheet.Cells[1, 3].Value = "Description";
                    worksheet.Cells[1, 4].Value = "CategoryId";
                    worksheet.Cells[1, 5].Value = "ImagePath";
                    worksheet.Cells[1, 6].Value = "Origin";
                    worksheet.Cells[1, 7].Value = "Model";
                    worksheet.Cells[1, 8].Value = "Occasion";
                    worksheet.Cells[1, 9].Value = "Style";
                    worksheet.Cells[1, 10].Value = "Material";

                    // Bat dau add du lieu
                    // luon nho rang Paginnation neu muon su dung gia tri Item thi can phai cau hinh lai Pagination o trong 
                    // o trong Domain chinh AddRange => thanh List<T> de lay duoc gia tri trong result khong thi an cut
                    for (int i = 0; i < result.Items.Count; i++)
                    {
                        worksheet.Cells[i + 2, 1].Value = result.Items[i].ProductId;
                        worksheet.Cells[i + 2, 2].Value = result.Items[i].Name;
                        worksheet.Cells[i + 2, 3].Value = result.Items[i].Description;
                        worksheet.Cells[i + 2, 4].Value = result.Items[i].CategoryId;
                        worksheet.Cells[i + 2, 5].Value = result.Items[i].ImagePath;
                        worksheet.Cells[i + 2, 6].Value = result.Items[i].Origin;
                        worksheet.Cells[i + 2, 7].Value = result.Items[i].Model;
                        worksheet.Cells[i + 2, 8].Value = result.Items[i].Occasion;
                        worksheet.Cells[i + 2, 9].Value = result.Items[i].Style;
                        worksheet.Cells[i + 2, 10].Value = result.Items[i].Material;
                    }

                    // dat kieu du lieu cho cac cot, bit, string, int , cai deo gi day
                    worksheet.Cells.AutoFitColumns();

                    // Create file Excel dưới dạng mảng byte
                    var excelData = package.GetAsByteArray();

                    // trai file
                    return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Products.xlsx");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageRespondDTO<object>(null, false, "An error occurred: " + ex.Message));
            }
        }
    }
}
