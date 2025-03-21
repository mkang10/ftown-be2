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
        public ProductController(
            GetAllProductsHandler getAllProductsHandler,
            GetProductDetailHandler getProductDetailHandler,
            GetProductVariantByIdHandler getProductVariantByIdHandler,
            GetAllProductVariantsByIdsHandler getAllProductVariantsByIdsHandler,
            GetProductVariantByDetailsHandler getAllProductVariantByDetailsHandler,
            CreateProductHandler createProductHandler,
            ElasticsearchService elasticsearchService)
        {
            _getAllProductsHandler = getAllProductsHandler;
            _getProductDetailHandler = getProductDetailHandler;
            _getProductVariantByIdHandler = getProductVariantByIdHandler;
            _getAllProductVariantsByIdsHandler = getAllProductVariantsByIdsHandler;
            _getProductVariantByDetailsHandler = getAllProductVariantByDetailsHandler;
            _createProductHandler = createProductHandler;
            _elasticsearchService = elasticsearchService;
        }

        [HttpGet("view-all")]
        public async Task<ActionResult<ResponseDTO<List<ProductListResponse>>>> GetAllProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var products = await _getAllProductsHandler.Handle(page, pageSize);

            if (products == null || !products.Any())
                return NotFound(new ResponseDTO<List<ProductListResponse>>(null, false, "Không có sản phẩm nào được tìm thấy."));

            return Ok(new ResponseDTO<List<ProductListResponse>>(products, true, "Lấy danh sách sản phẩm thành công!"));
        }


        [HttpGet("{productId}")]
        public async Task<ActionResult<ResponseDTO<ProductDetailResponse>>> GetProductDetail(int productId)
        {
            var product = await _getProductDetailHandler.Handle(productId);

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

        
        //[HttpPut("variant/update")]
        //public async Task<ActionResult<ResponseDTO<bool>>> UpdateProductVariant([FromBody] ProductVariantRequest request)
        //{
        //    var productVariant = _mapper.Map<ProductVariant>(request);

        //    var result = await _updateProductVariantHandler.Handle(productVariant);

        //    if (!result)
        //        return BadRequest(new ResponseDTO<bool>(false, false, "Cập nhật biến thể sản phẩm thất bại."));

        //    return Ok(new ResponseDTO<bool>(true, true, "Cập nhật biến thể sản phẩm thành công!"));
        //}

        //[HttpGet("search")]
        //public async Task<ActionResult<List<ProductListResponse>>> SearchProducts([FromQuery] string query)
        //{
        //    if (string.IsNullOrEmpty(query))
        //        return BadRequest("Query không được để trống.");

        //    string cacheKey = $"search:{query}";

        //    // Kiểm tra cache trước khi tìm kiếm trong Elasticsearch
        //    var cachedResults = await _redisCacheService.GetCacheAsync<List<ProductListResponse>>(cacheKey);
        //    if (cachedResults != null)
        //        return Ok(cachedResults);

        //    // Nếu không có cache, tìm kiếm trong Elasticsearch
        //    var results = await _elasticsearchService.SearchProductsAsync(query);

        //    // Lưu kết quả vào Redis với TTL 5 phút
        //    await _redisCacheService.SetCacheAsync(cacheKey, results, TimeSpan.FromMinutes(5));

        //    return Ok(results);
        //}

    }
}

