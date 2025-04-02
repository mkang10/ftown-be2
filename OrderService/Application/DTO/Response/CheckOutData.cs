using Application.DTO.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Response
{
    public class CheckOutData
    {
        public int AccountId { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public int? ShippingAddressId { get; set; }
        public int? WarehouseId { get; set; } // ✅ Thêm StoreId để tránh lỗi CS1061
        public List<OrderItemRequest> Items { get; set; } = new();
    }
}
