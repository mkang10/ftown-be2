﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Response
{
    public class OrderResponse
    {
        public int OrderId { get; set; }
        public string Status { get; set; } = "Pending";
        public decimal OrderTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string? PaymentUrl { get; set; } // Nếu là PAYOS, URL để thanh toán
        public DateTime? DeliveredDate { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new();
    }
}
