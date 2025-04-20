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
using OfficeOpenXml.Style;
using System.Drawing;

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
                var products = await _repo.GetAllProductList();

                if (products == null || !products.Any())
                {
                    return NotFound(new MessageRespondDTO<object>(null, false, "Không có sản phẩm nào để xuất."));
                }

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Danh sách sản phẩm");

                // Header tiếng Việt
                var headers = new[]
                {
            "Mã sản phẩm", "Tên sản phẩm", "Mô tả", "Danh mục", "Xuất xứ", "Model", "Dịp sử dụng", "Phong cách", "Chất liệu",
            "Trạng thái", "Giá", "Ảnh đại diện", "SKU", "Mã vạch", "Khối lượng", "Kích cỡ", "Màu sắc", "Mã màu"
        };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cells[1, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);
                }

                var row = 2;
                foreach (var product in products)
                {
                    foreach (var variant in product.ProductVariants)
                    {
                        var sizeName = variant.Size?.SizeName;
                        var colorName = variant.Color?.ColorName;
                        var colorCode = variant.Color?.ColorCode;

                        worksheet.Cells[row, 1].Value = product.ProductId;
                        worksheet.Cells[row, 2].Value = product.Name;
                        worksheet.Cells[row, 3].Value = product.Description;
                        worksheet.Cells[row, 4].Value = product.Category?.Name;
                        worksheet.Cells[row, 5].Value = product.Origin;
                        worksheet.Cells[row, 6].Value = product.Model;
                        worksheet.Cells[row, 7].Value = product.Occasion;
                        worksheet.Cells[row, 8].Value = product.Style;
                        worksheet.Cells[row, 9].Value = product.Material;
                        worksheet.Cells[row, 10].Value = product.Status;
                        worksheet.Cells[row, 11].Value = variant.Price;
                        worksheet.Cells[row, 12].Value = variant.ImagePath;
                        worksheet.Cells[row, 13].Value = variant.Sku;
                        worksheet.Cells[row, 14].Value = variant.Barcode;
                        worksheet.Cells[row, 15].Value = variant.Weight;
                        worksheet.Cells[row, 16].Value = sizeName;
                        worksheet.Cells[row, 17].Value = colorName;
                        worksheet.Cells[row, 18].Value = colorCode;

                        row++;
                    }
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var fileContents = package.GetAsByteArray();
                var fileName = $"DanhSachSanPham_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageRespondDTO<object>(null, false, $"Đã xảy ra lỗi: {ex.Message}"));
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
