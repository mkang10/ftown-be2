using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Response
{
    public class OrderDetailResponse
    {
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public decimal DiscountApplied { get; set; }

        // Các thông tin được enrich từ ProductVariant
        public string ProductName { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
    }
}
