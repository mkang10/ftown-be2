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
        public string? CategoryName { get; set; }
        public List<string>? Colors { get; set; } // Danh sách màu sắc có sẵn
    }
}
