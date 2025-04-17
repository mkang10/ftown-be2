using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Response
{
    public class ProductListResponse
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string? ImagePath { get; set; }
        public decimal Price { get; set; } // Giá của sản phẩm
        public decimal DiscountedPrice { get; set; } // Giá sau khi áp dụng khuyến mãi
        public string? CategoryName { get; set; }
        public string? PromotionTitle { get; set; } // Tên khuyến mãi nếu có
        public bool? IsFavorite { get; set; }
        public List<string> Colors { get; set; } = new();
    }
    public class ColorInfo
    {
        public int ColorId { get; set; }
        public string ColorName { get; set; }
        public string? ColorCode { get; set; }
    }
}
