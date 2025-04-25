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
        private readonly GetAllProductHandler _getallProductHandler;
        private readonly GetProductDetailHandler _detailHandler;
        private readonly EditProductHandler _editProductHandler;
        private readonly GetVariantHandler _getVariantHandler;
        private readonly EditVariantHandler _editVariantHandler;
        private readonly RedisHandler _redisHandler;



        public ProductsController(RedisHandler redisHandler ,EditVariantHandler editVariantHandler ,GetVariantHandler getVariantHandler, EditProductHandler editProductHandler, GetProductDetailHandler detailHandler, CreateProductHandler createProductHandler, GetAllProductHandler getAllProductHandler)
        {
            _createProductHandler = createProductHandler;
            _getallProductHandler = getAllProductHandler;
            _detailHandler = detailHandler;
            _editProductHandler = editProductHandler;
            _getVariantHandler = getVariantHandler;
            _editVariantHandler = editVariantHandler;
            _redisHandler = redisHandler;
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

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductWithVariants(int productId)
        {
            var result = await _detailHandler.GetProductWithVariantsAsync(productId);

            if (!result.Status)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("variant-detail")]
        public async Task<IActionResult> GetDetail(int variantId)
        {
            var response = await _getVariantHandler.GetProductVariantDetailAsync(variantId);
            if (!response.Status)
                return NotFound(response);

            return Ok(response);
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

        [HttpPut]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Edit([FromForm] EditProductVariantDto dto)
        {
            // 1. Thực hiện cập nhật variant
            var result = await _editVariantHandler.EditProductVariantAsync(dto);
            if (!result.Status)
                return NotFound(result);

            // 2. Nếu thành công, clear cache
            string instanceName = "ProductInstance";  // 👈 Đặt InstanceName của bạn tại đây
            var resultMessage = await _redisHandler.ClearInstanceCacheAsync(instanceName);

            // 3. Trả về message từ Redis handler
            return Ok(new { Message = resultMessage });
        }


        [HttpGet("color")]
        public async Task<IActionResult> GetColors()
        {
            var result = await _editVariantHandler.GetAllColorsByProductAsync();
            return Ok(result);
        }

        [HttpGet("size")]
        public async Task<IActionResult> GetSizes()
        {
            var result = await _editVariantHandler.GetAllSizesByProductAsync();
            return Ok(result);
        }


        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Edit(int id, [FromForm] ProductEditDto dto)
        {
            try
            {
                // 1. Cập nhật product
                await _editProductHandler.EditAsync(id, dto);

                // 2. Nếu thành công, clear cache
                string instanceName = "ProductInstance";  // 👈 Đặt InstanceName tại đây
                var resultMessage = await _redisHandler.ClearInstanceCacheAsync(instanceName);

                // 3. Trả về message từ Redis handler
                return Ok(new { Message = resultMessage });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ResponseDTO<string>(
                    null,
                    false,
                    $"Product with id={id} not found"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<string>(
                    null,
                    false,
                    $"An error occurred: {ex.Message}"
                ));
            }
        }



    }

}
