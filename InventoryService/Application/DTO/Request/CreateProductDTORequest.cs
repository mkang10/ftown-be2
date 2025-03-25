using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Request
{
    public class CreateProductDTORequest
    {
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public int? CategoryId { get; set; }

        public string? ImagePath { get; set; }

        public string? Origin { get; set; }

        public string? Model { get; set; }

        public string? Occasion { get; set; }

        public string? Style { get; set; }

        public string? Material { get; set; }
    }
}
