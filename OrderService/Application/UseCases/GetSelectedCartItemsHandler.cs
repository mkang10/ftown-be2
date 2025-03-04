using Application.DTO.Request;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetSelectedCartItemsHandler
    {
        private readonly ICustomerServiceClient _customerServiceClient;
        private readonly IInventoryServiceClient _inventoryServiceClient;

        public GetSelectedCartItemsHandler(ICustomerServiceClient customerServiceClient, IInventoryServiceClient inventoryServiceClient)
        {
            _customerServiceClient = customerServiceClient;
            _inventoryServiceClient = inventoryServiceClient;
        }

        public async Task<List<OrderItemRequest>?> Handle(int accountId, List<int> selectedProductVariantIds)
        {
            // ✅ 1. Lấy toàn bộ giỏ hàng từ CustomerService
            var cartItems = await _customerServiceClient.GetCartAsync(accountId);
            if (cartItems == null || !cartItems.Any())
                return null; // Nếu giỏ hàng rỗng, trả về null

            var orderItems = new List<OrderItemRequest>();

            // ✅ 2. Lọc ra các sản phẩm được chọn từ giỏ hàng
            foreach (var productVariantId in selectedProductVariantIds)
            {
                var cartItem = cartItems.FirstOrDefault(c => c.ProductVariantId == productVariantId);
                if (cartItem == null)
                    return null; // Nếu sản phẩm không có trong giỏ hàng, trả về null

                // ✅ 3. Lấy thông tin chi tiết sản phẩm từ InventoryService
                var productVariant = await _inventoryServiceClient.GetProductVariantByIdAsync(cartItem.ProductVariantId);
                if (productVariant == null)
                    return null; // Nếu không tìm thấy sản phẩm trong kho

                orderItems.Add(new OrderItemRequest
                {
                    ProductVariantId = cartItem.ProductVariantId,
                    ProductName = productVariant.ProductName,
                    Size =productVariant.Size,
                    Color = productVariant.Color,
                    Quantity = cartItem.Quantity, // ✅ Tự động lấy số lượng từ giỏ hàng
                    Price = productVariant.Price

                });
            }

            return orderItems;
        }
    }
}
