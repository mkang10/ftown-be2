using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPayOSService
    {
        Task<string?> CreatePayment(int orderId, decimal amount, string paymentMethod);
    }
}
