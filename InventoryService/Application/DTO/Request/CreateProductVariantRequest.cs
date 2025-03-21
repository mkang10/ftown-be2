using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Request
{
    public class CreateProductVariantRequest
    {
        public decimal Price { get; set; }
        public string? ImagePath { get; set; }
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public decimal? Weight { get; set; }
        public int? SizeId { get; set; }
        public int? ColorId { get; set; }
    }
}
