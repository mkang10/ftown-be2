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
    public class TransferRepository : ITransferRepository
    {
        private readonly FtownContext _context;
        public TransferRepository(FtownContext context) => _context = context;
        public async Task AddAsync(Transfer transfer)
            => await _context.Transfers.AddAsync(transfer);


        public async Task UpdateAsync(Transfer entity)
        {
            // Đánh dấu entity đã thay đổi
            _context.Transfers.Update(entity);
            // Lưu thay đổi xuống CSDL
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<TransferDetail> entities)
        {
            // EF Core đã hỗ trợ AddRangeAsync
            await _context.Set<TransferDetail>().AddRangeAsync(entities);
        }

    }

}
