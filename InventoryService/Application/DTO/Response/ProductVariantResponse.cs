﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Response
{
    public class ProductVariantResponse
    {
        public int VariantId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Size { get; set; }
        public string? Color { get; set; }
        public decimal Price { get; set; }
        public int? StockQuantity { get; set; }
        public string? ImagePath { get; set; }
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public decimal? Weight { get; set; }
    }
}
