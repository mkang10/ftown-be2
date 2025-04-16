using Application.DTO.Request;
using Application.DTO.Response;
using Application.UseCases;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nest;

namespace API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly GetAllProductsHandler _getAllProductsHandler;
        private readonly GetProductDetailHandler _getProductDetailHandler;
        private readonly GetProductVariantByIdHandler _getProductVariantByIdHandler;
        private readonly GetAllProductVariantsByIdsHandler _getAllProductVariantsByIdsHandler;
        private readonly GetProductVariantByDetailsHandler _getProductVariantByDetailsHandler;
        private readonly CreateProductHandler _createProductHandler;
        private readonly ElasticsearchService _elasticsearchService;
        private readonly FilterProductHandler _filterProductHandler;
        private readonly GetTopSellingProductHandler _getTopSellingProductHandler;
        public ProductController(
            GetAllProductsHandler getAllProductsHandler,
            GetProductDetailHandler getProductDetailHandler,
            GetProductVariantByIdHandler getProductVariantByIdHandler,
            GetAllProductVariantsByIdsHandler getAllProductVariantsByIdsHandler,
            GetProductVariantByDetailsHandler getAllProductVariantByDetailsHandler,
            CreateProductHandler createProductHandler,
            ElasticsearchService elasticsearchService,
            FilterProductHandler filterProductHandler,
            GetTopSellingProductHandler getTopSellingProductHandler)
        {
            _getAllProductsHandler = getAllProductsHandler;
            _getProductDetailHandler = getProductDetailHandler;
            _getProductVariantByIdHandler = getProductVariantByIdHandler;
            _getAllProductVariantsByIdsHandler = getAllProductVariantsByIdsHandler;
            _getProductVariantByDetailsHandler = getAllProductVariantByDetailsHandler;
            _createProductHandler = createProductHandler;
            _elasticsearchService = elasticsearchService;
            _filterProductHandler = filterProductHandler;
            _getTopSellingProductHandler = getTopSellingProductHandler;
        }

        [HttpGet("view-all")]
        public async Task<ActionResult<ResponseDTO<List<ProductListResponse>>>> GetAllProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var products = await _getAllProductsHandler.Handle(page, pageSize);

            if (products == null || !products.Any())
                return NotFound(new ResponseDTO<List<ProductListResponse>>(null, false, "Không có sản phẩm nào được tìm thấy."));

            return Ok(new ResponseDTO<List<ProductListResponse>>(products, true, "Lấy danh sách sản phẩm thành công!"));
        }
        [HttpGet("filter-by-category")]
        public async Task<ActionResult<ResponseDTO<List<ProductListResponse>>>> FilterProductsByCategory([FromQuery] string categoryName)
        {
            var products = await _filterProductHandler.Handle(categoryName);

            if (products == null || !products.Any())
                return NotFound(new ResponseDTO<List<ProductListResponse>>(null, false, "Không có sản phẩm nào thuộc danh mục này."));

            return Ok(new ResponseDTO<List<ProductListResponse>>(products, true, "Lọc sản phẩm theo danh mục thành công!"));
        }

        [HttpGet("{productId}")]
		public async Task<ActionResult<ResponseDTO<ProductDetailResponse>>> GetProductDetail(int productId, [FromQuery] int? accountId)
		{
			var product = await _getProductDetailHandler.Handle(productId, accountId);

			if (product == null)
				return NotFound(new ResponseDTO<ProductDetailResponse>(null, false, "Không tìm thấy sản phẩm!"));

			return Ok(new ResponseDTO<ProductDetailResponse>(product, true, "Lấy chi tiết sản phẩm thành công!"));
		}



		[HttpGet("variant/{variantId}")]
        public async Task<ActionResult<ResponseDTO<ProductVariantResponse>>> GetProductVariantById(int variantId)
        {
            var variant = await _getProductVariantByIdHandler.Handle(variantId);

            if (variant == null)
                return NotFound(new ResponseDTO<ProductVariantResponse>(null, false, "Không tìm thấy biến thể sản phẩm."));

            return Ok(new ResponseDTO<ProductVariantResponse>(variant, true, "Lấy biến thể sản phẩm thành công!"));
        }

        [HttpPost("variants/details")]
        public async Task<ActionResult<ResponseDTO<List<ProductVariantResponse>>>> GetAllProductVariantsByIdsAsync([FromBody] List<int> variantIds)
        {
            var variants = await _getAllProductVariantsByIdsHandler.Handle(variantIds);

            if (variants == null || variants.Count == 0)
                return NotFound(new ResponseDTO<List<ProductVariantResponse>>(null, false, "Không tìm thấy biến thể sản phẩm nào."));

            return Ok(new ResponseDTO<List<ProductVariantResponse>>(variants, true, "Lấy danh sách biến thể sản phẩm thành công!"));
        }

        [HttpGet("variant/details")]
        public async Task<ActionResult<ResponseDTO<ProductVariantResponse>>> GetProductVariantByDetails([FromQuery] GetProductVariantByDetailsRequest request)
        {
            var variant = await _getProductVariantByDetailsHandler.HandleAsync(request);

            if (variant == null)
                return NotFound(new ResponseDTO<ProductVariantResponse>(null, false, "Không tìm thấy biến thể sản phẩm."));

            return Ok(new ResponseDTO<ProductVariantResponse>(variant, true, "Lấy biến thể sản phẩm thành công!"));
        }

        // Endpoint tạo nhiều sản phẩm cùng lúc
        [HttpPost("create")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Create([FromForm] CreateMultipleProductsRequest model)
        {
            if (model.Requests == null || !model.Requests.Any())
                return BadRequest(new ResponseDTO(false, "Không có dữ liệu sản phẩm"));

            try
            {
                var result = await _createProductHandler.CreateMultipleProductsAsync(model.Requests);
                return Ok(new ResponseDTO<List<ProductDetailResponse>>(result, true, $"Tạo thành công {result.Count} sản phẩm"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO(false, $"Lỗi: {ex.Message}"));
            }
        }
        [HttpGet("top-selling-products")]
        public async Task<ActionResult<ResponseDTO<List<TopSellingProductResponse>>>> GetTopSellingProducts(
                                        [FromQuery] DateTime? from,
                                        [FromQuery] DateTime? to,
                                        [FromQuery] int top = 10)
        {
            var result = await _getTopSellingProductHandler.GetTopSellingProductsAsync(from, to, top);
            return Ok(result);
        }


    }
}

