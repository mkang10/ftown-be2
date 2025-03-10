﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Response
{
    public class ProductDetailResponse
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        public string? ImagePath { get; set; } // Ảnh chính (có thể giữ lại để tương thích)
        public List<string>? ImagePaths { get; set; } = new(); // 👈 Thêm danh sách ảnh

        public string? Origin { get; set; }
        public string? Model { get; set; }
        public string? Occasion { get; set; }
        public string? Style { get; set; }
        public string? Material { get; set; }
        public string? CategoryName { get; set; }

        public List<ProductVariantResponse> Variants { get; set; } = new();
    }
}
