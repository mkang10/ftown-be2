using System;
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

        public string? ImagePath { get; set; } 
        public List<string>? ImagePaths { get; set; } = new(); 

        public string? Origin { get; set; }
        public string? Model { get; set; }
        public string? Occasion { get; set; }
        public string? Style { get; set; }
        public string? Material { get; set; }
        public string? CategoryName { get; set; }
		public bool? IsFavorite { get; set; }
		public List<ProductVariantResponse> Variants { get; set; } = new();
    }
}
