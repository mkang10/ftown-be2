using Application.DTO.Request;
using Application.DTO.Response;
using Application.UseCases;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly CartHandler _cartHandler;
        private readonly ICartRepository _cartRepository;
        public CartController(CartHandler cartHandler, ICartRepository cartRepository)
        {
            _cartHandler = cartHandler;
            _cartRepository = cartRepository;
        }

        [HttpGet("{accountId}")]
        public async Task<ActionResult<List<CartItemResponse>>> GetCart(int accountId)
        {
            var cartItems = await _cartHandler.GetShoppingCart(accountId);
            return Ok(cartItems);  // Không serialize JSON thủ công
        }


        [HttpPost("{accountId}/add")]
        public async Task<IActionResult> AddToCart(int accountId, [FromBody] AddToCartRequest cartItem)
        {
            await _cartHandler.AddCartItem(accountId, cartItem);
            return Ok();
        }

        [HttpDelete("{accountId}/remove/{productVariantId}")]
        public async Task<IActionResult> RemoveFromCart(int accountId, int productVariantId)
        {
            await _cartHandler.RemoveCartItem(accountId, productVariantId);
            return Ok(new { message = "Đã cập nhật giỏ hàng trên Redis thành công." });
        }


        [HttpDelete("{accountId}/clear")]
        public async Task<IActionResult> ClearCart(int accountId)
        {
            await _cartHandler.ClearCart(accountId);
            return Ok();
        }

        [HttpPost("sync-cart/{accountId}")]
        public async Task<IActionResult> SyncCartToDatabase(int accountId)
        {
            await _cartHandler.SyncCartToDatabase(accountId);
            return Ok("Đã đồng bộ giỏ hàng vào Database.");
        }

        [HttpPost("{accountId}/clear-after-order")]
        public async Task<IActionResult> ClearCartAfterOrder(int accountId)
        {
            await _cartHandler.ClearCartAfterOrderAsync(accountId);
            return Ok(new { message = "Giỏ hàng đã được xóa sau khi tạo đơn hàng thành công." });
        }
    }
}
