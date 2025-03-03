using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Request
{
    public class CreateOrderRequest
    {
        public int AccountId { get; set; }
        public string CheckOutSessionId { get; set; } = null!;
        public int? ShippingAddressId { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public int? StoreId { get; set; }
        //public List<int> SelectedProductVariantIds { get; set; } = new();
    }
}
