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
        public decimal MinPrice { get; set; } // Giá thấp nhất của biến thể
        public decimal MaxPrice { get; set; } // Giá cao nhất của biến thể
        public string? CategoryName { get; set; }
        public List<string>? Colors { get; set; } // Danh sách màu sắc có sẵn
    }
}
