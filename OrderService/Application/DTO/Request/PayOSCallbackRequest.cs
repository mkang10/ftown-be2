using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Request
{
    public class PayOSCallbackRequest
    {
        public int OrderId { get; set; }
        public string Status { get; set; } = null!;
       // public string TransactionId { get; set; } = null!;
    }
}
