using Domain.DTO.Request;
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
    public class StaffDetailRepository : IStaffDetailRepository
    {
        private readonly FtownContext _context;

        public StaffDetailRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<StaffDetail?> GetByIdAsync(int staffDetailId)
        {
            return await _context.StaffDetails.FindAsync(staffDetailId);
        }

        public async Task<StaffDetail?> GetByAccountIdAsync(int accountId)
        {
            return await _context.StaffDetails
                  .Include(s => s.Account)
                  .FirstOrDefaultAsync(s => s.Account.AccountId == accountId);
        }

        public async Task<IEnumerable<StaffNameDto>> GetAllStaffNamesAsync(int warehouseId)
        {
            return await _context.StaffDetails
                .Include(s => s.Account)
                .Where(s => s.StoreId == warehouseId) // lọc theo warehouse
                .GroupBy(s => new { s.StaffDetailId, s.Account.FullName })
                .Select(g => new StaffNameDto
                {
                    Id = g.Key.StaffDetailId,
                    FullName = g.Key.FullName
                })
                .ToListAsync();
        }


        public async Task<StaffDetail?> GetByAcIdAsync(int accountId)
        {
            return await _context.StaffDetails
                  .Include(s => s.Account)
                  .FirstOrDefaultAsync(s => s.Account.AccountId == accountId);
        }




    }
}
