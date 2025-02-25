using Application.DTO.Response;
using Application.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly GetAllProductsHandler _getAllProductsHandler;
        private readonly GetProductDetailHandler _getProductDetailHandler;
        private readonly GetProductVariantByIdHandler _getProductVariantByIdHandler;
        public ProductController(GetAllProductsHandler getAllProductsHandler, GetProductDetailHandler getProductDetailHandler, GetProductVariantByIdHandler getProductVariantByIdHandler)
        {
            _getAllProductsHandler = getAllProductsHandler;
            _getProductDetailHandler = getProductDetailHandler;
            _getProductVariantByIdHandler = getProductVariantByIdHandler;
        }

        [HttpGet("view-all")]
        public async Task<ActionResult<List<ProductListResponse>>> GetAllProducts()
        {
            var products = await _getAllProductsHandler.Handle();
            return Ok(products);
        }

        [HttpGet("{productId}")]
        public async Task<ActionResult<ProductDetailResponse>> GetProductDetail(int productId)
        {
            var product = await _getProductDetailHandler.Handle(productId);
            if (product == null) return NotFound();
            return Ok(product);
        }
        [HttpGet("variant/{variantId}")]
        public async Task<ActionResult<ProductVariantResponse>> GetProductVariantById(int variantId)
        {
            var variant = await _getProductVariantByIdHandler.Handle(variantId);

            if (variant == null)
                return NotFound("Không tìm thấy biến thể sản phẩm.");

            return Ok(variant);
        }
    }
}
