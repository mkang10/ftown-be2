﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.DTO.Response
{
    public class OrderDetailResponseWrapper
    {
        public int OrderId { get; set; }

        // Thông tin giao hàng từ Order
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string? Country { get; set; }

        // Phương thức thanh toán
        public string PaymentMethod { get; set; } = string.Empty;

        public StoreResponse? Store { get; set; }

        // Tổng tiền đơn hàng (đã lưu sẵn trong DB)
        public decimal OrderTotal { get; set; }
        public decimal ShippingCost { get; set; }

        public List<OrderItemResponse> OrderItems { get; set; } = new();
    }

}
