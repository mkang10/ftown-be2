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
    public class DispatchRepos : IDispatchRepos { 
    
        
    private readonly FtownContext _context;
        public DispatchRepos(FtownContext context)
        {
            _context = context;
        }

        public void Add(Dispatch dispatch)
        {
            _context.Dispatches.Add(dispatch);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public async Task<Dispatch?> GetDispatchByTransferIdAsync(int transferId)
        {
            var transfer = await _context.Transfers
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TransferOrderId == transferId);
            if (transfer == null || transfer.DispatchId == 0)
                return null;

            return await _context.Dispatches
                .Include(d => d.DispatchDetails)
                    .ThenInclude(dd => dd.Variant)
                        .ThenInclude(v => v.Product)
                .Include(d => d.DispatchDetails)
                    .ThenInclude(dd => dd.Variant)
                        .ThenInclude(v => v.Color)
                .Include(d => d.DispatchDetails)
                    .ThenInclude(dd => dd.Variant)
                        .ThenInclude(v => v.Size)
                .Include(d => d.DispatchDetails)
                    .ThenInclude(dd => dd.StoreExportStoreDetails)
                .FirstOrDefaultAsync(d => d.DispatchId == transfer.DispatchId);
        }
    }

}
