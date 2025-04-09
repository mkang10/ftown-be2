using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Response
{
    public class PaymentCreationResult
    {
        public string? CheckoutUrl { get; set; }
        public long OrderCode { get; set; }
    }
}
