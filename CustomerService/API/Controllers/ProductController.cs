using Application.DTO.Response;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly IInventoryServiceClient _inventoryServiceClient;

        public ProductController(IInventoryServiceClient inventoryServiceClient)
        {
            _inventoryServiceClient = inventoryServiceClient;
        }

        /// <summary>
        /// Lấy danh sách toàn bộ sản phẩm từ InventoryService
        /// </summary>
        [HttpGet("view-all")]
        public async Task<ActionResult<List<ProductResponse>>> GetAllProducts()
        {
            var products = await _inventoryServiceClient.GetAllProductsAsync();
            if (products == null || products.Count == 0)
            {
                return NotFound("Không tìm thấy sản phẩm nào.");
            }
            return Ok(products);
        }
    }
}
