using Application.UseCases;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly CreateProductHandler _createProductHandler;
        private readonly GetAllProductHandler  _getallProductHandler;

        public ProductsController(CreateProductHandler createProductHandler, GetAllProductHandler getAllProductHandler)
        {
            _createProductHandler = createProductHandler;
            _getallProductHandler = getAllProductHandler;
        }

        // POST: api/Products
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] ProductCreateDto dto)
        {
            try
            {
                int productId = await _createProductHandler.CreateProductAsync(dto);
                var response = new ResponseDTO<int>(
                    productId,
                    true,
                    "Product created successfully."
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log exception nếu cần
                var response = new ResponseDTO<int>(
                    0,
                    false,
                    $"An error occurred while creating the product: {ex.Message}"
                );
                return StatusCode(500, response);
            }
        }

        // POST: api/Products/variant
        [HttpPost("variant")]
        public async Task<IActionResult> CreateVariant([FromForm] ProductVariantCreateDto dto)
        {
            try
            {
                int variantId = await _createProductHandler.CreateVariantAsync(dto);
                var response = new ResponseDTO<int>(
                    variantId,
                    true,
                    "Product variant created successfully."
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseDTO<int>(
                    0,
                    false,
                    $"An error occurred while creating the product variant: {ex.Message}"
                );
                return StatusCode(500, response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponseDTO<ProductDto>>>
            GetAll(
                [FromQuery] string? name,
                [FromQuery] string? description,
                [FromQuery] int? category,
                [FromQuery] string? origin,
                [FromQuery] string? model,
                [FromQuery] string? occasion,
                [FromQuery] string? style,
                [FromQuery] string? material,
                [FromQuery] string? status,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10)
        {
            var result = await _getallProductHandler.GetAllProductsAsync(
                name,
                description,
                category,
                origin,
                model,
                occasion,
                style,
                material,
                status,
                page,
                pageSize);
            return Ok(result);
        }
    }
}
