using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class EditProfileRepository : IEditProfileRepository
    {
        private readonly FtownContext _context;

        public EditProfileRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetAccountByIdAsync(int accountId)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId);
        }

        public async Task<CustomerDetail?> GetCustomerDetailByAccountIdAsync(int accountId)
        {
            return await _context.CustomerDetails.FirstOrDefaultAsync(cd => cd.AccountId == accountId);
        }

        public async Task UpdateAccountAsync(Account account)
        {
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCustomerDetailAsync(CustomerDetail customerDetail)
        {
            _context.CustomerDetails.Update(customerDetail);
            await _context.SaveChangesAsync();
        }
        public async Task<(Account?, CustomerDetail?)> GetCustomerProfileByAccountIdAsync(int accountId)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId);
            var customerDetail = await _context.CustomerDetails.FirstOrDefaultAsync(cd => cd.AccountId == accountId);
            return (account, customerDetail);
        }
    }
}
