using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Request
{
    public class CreateMultipleProductsRequest
    {
        public List<CreateProductRequest> Requests { get; set; }
    }

}
