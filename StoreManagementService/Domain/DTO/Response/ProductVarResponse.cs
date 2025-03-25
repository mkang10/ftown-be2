//using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    
        public class ProductVariantResponseDto
        {
            public int VariantId { get; set; }
            public string ProductName { get; set; } = null!;
        }
    


}
