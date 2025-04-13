using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Response
{
    public class TopSellingProductResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public string? ImagePath { get; set; }
    }

}
