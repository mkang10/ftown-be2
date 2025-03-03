using Application.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Request
{
    public class OrderItemRequest
    {
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ProductName { get; set; } // Tên sản phẩm
        public string ImageUrl { get; set; } // Ảnh sản phẩm
        public string Size { get; set; } // Kích thước của variant
        public string Color { get; set; } // Màu sắc của variant
    }
}
