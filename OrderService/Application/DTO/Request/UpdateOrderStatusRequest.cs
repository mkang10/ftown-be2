using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Request
{
    public class UpdateOrderStatusRequest
    {
        public long OrderId { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
    }
}
