using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IProfileRepository
    {
        Task<Account?> GetAccountByIdAsync(int accountId);
        Task<CustomerDetail?> GetCustomerDetailByAccountIdAsync(int accountId);
        Task UpdateAccountAsync(Account account);
        Task UpdateCustomerDetailAsync(CustomerDetail customerDetail);
        Task<(Account?, CustomerDetail?)> GetCustomerProfileByAccountIdAsync(int accountId);
    }
}
