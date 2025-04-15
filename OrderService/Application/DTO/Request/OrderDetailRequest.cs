using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Request
{
    public class OrderDetailRequest
    {
        public string order_code { get; set; }
    }

    public class OrderDetailWithUpdateRequest
    {
        public string order_code { get; set; }
        public int create_by { get; set; }


    }
}
