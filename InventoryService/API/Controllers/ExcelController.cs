using Application.DTO.Request;
using Application.DTO.Response;
using Application.Enum;
using Application.Interfaces;
using AutoMapper;
using Domain.Commons;
using Domain.Entities;
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
        private readonly IMapper _mapper;

        public ExcelController(IExcelService service, IExcelRepo repo, IMapper mapper)
        {
            _service = service;
            _repo = repo;
            _mapper = mapper;
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
        [HttpPost("create")]
        public async Task<IActionResult> CreateProduct(List<CreateProductDTORequest> user)
        {
            try
            {
                var data = await _service.CreateProduct(user);
                if (data == null)
                {
                    return BadRequest(new MessageRespondDTO<object>(null, false, StatusSuccess.Wrong.ToString()));
                }
                return Ok(new MessageRespondDTO<List<CreateProductDTORequest>>(data, true, StatusSuccess.Success.ToString()));
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageRespondDTO<object>(null, false, ex.Message));
            }
        }

        [HttpGet("export-excel")]
        public async Task<IActionResult> Export([FromQuery] PaginationParameter paginationParameter)
        {
            try
            {
                var result = await _repo.GetAllProduct(paginationParameter);

                // LicenseContext de lach luat
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
                    // o trong Domain chinh AddRange => thanh List<T> de lay duoc gia tri trong result 
                    for (int i = 0; i < result.Items.Count; i++)
                    {
                        worksheet.Cells[i + 2, 1].Value = result.Items[i].ProductId;
                        worksheet.Cells[i + 2, 2].Value = result.Items[i].Name;
                        worksheet.Cells[i + 2, 3].Value = result.Items[i].Description;
                        worksheet.Cells[i + 2, 4].Value = result.Items[i].CategoryId;
                        //worksheet.Cells[i + 2, 5].Value = result.Items[i].ImagePath;
                        worksheet.Cells[i + 2, 6].Value = result.Items[i].Origin;
                        worksheet.Cells[i + 2, 7].Value = result.Items[i].Model;
                        worksheet.Cells[i + 2, 8].Value = result.Items[i].Occasion;
                        worksheet.Cells[i + 2, 9].Value = result.Items[i].Style;
                        worksheet.Cells[i + 2, 10].Value = result.Items[i].Material;
                    }

                    // dat kieu du lieu cho cac cot, bit, string, int ,....
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
        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportExcel([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new MessageRespondDTO<object>(null, false, "An error occur: No file was upload"));
            }

            try
            {
                // them lincense micosof
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            return BadRequest(new MessageRespondDTO<object>(null, false, "An Error occur: Can not find worksheet in file!"));
                        }

                        var productList = new List<Product>();

                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++) 
                        {
                            var product = new Product
                            {
                                // xoa cai id di
                                Name = worksheet.Cells[row, 2].Text,
                                Description = worksheet.Cells[row, 3].Text,
                                CategoryId = int.Parse(worksheet.Cells[row, 4].Text),
                                //ImagePath = worksheet.Cells[row, 5].Text,
                                Origin = worksheet.Cells[row, 6].Text,
                                Model = worksheet.Cells[row, 7].Text,
                                Occasion = worksheet.Cells[row, 8].Text,
                                Style = worksheet.Cells[row, 9].Text,
                                Material = worksheet.Cells[row, 10].Text
                            };

                            productList.Add(product);
                        }

                        // Ánh xạ từ List<Product> sang List<CreateProductDTORequest>
                        var dtoList = _mapper.Map<List<CreateProductDTORequest>>(productList);

                        await _service.CreateProduct(dtoList);

                        return Ok(new MessageRespondDTO<object>(dtoList, true, "Success"));
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageRespondDTO<object>(null, false, "An error occur: " + ex.Message));
            }
        }
    }

}
