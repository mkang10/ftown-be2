using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Response
{
    public class SubmitReturnResponse
    {
        public int ReturnOrderId { get; set; }
        public string Status { get; set; } = "Pending";
    }

}
