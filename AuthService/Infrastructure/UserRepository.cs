using Application.Interfaces;
using Domain.Entities;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

namespace Infrastructure.Repositories
{
    public class AccountRepos : IAccountRepos
    {
        private readonly FtownContext _context;

        public AccountRepos(FtownContext context)
        {
            _context = context;
        }

        public async Task<Account> GetUserByUsernameAsync(string username)
        {
            return await _context.Accounts.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.FullName == username);
        }

        public async Task AddUserAsync(Account acc)
        {
            await _context.Accounts.AddAsync(acc);
            await _context.SaveChangesAsync();
        }
    }
}
