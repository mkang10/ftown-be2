﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class ProductDetailDTO
    {
        public class ProductDTO
        {
            public int ProductId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            // ... các field cần thiết khác
        }

        public class ProductVariantDto
        {
            public int VariantId { get; set; }
            public int SizeId { get; set; }
            public int ColorId { get; set; }
            public decimal Price { get; set; }
            public string? ImagePath { get; set; }
            public string Sku { get; set; } = string.Empty;
            // ... các field khác
        }

        public class ProductWithVariantsDto
        {
            public ProductDto Product { get; set; } = null!;
            public int[] VariantIds { get; set; } = Array.Empty<int>();
            public List<ProductVariantDto> Variants { get; set; } = new();
        }
    }
}
