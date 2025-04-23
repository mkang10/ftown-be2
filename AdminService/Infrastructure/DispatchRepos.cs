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
        // =============Duc Anh 12-04-2025 22:34================
        public async Task<Dispatch> GetJSONDispatchById(int id) // Truyen DispatchId vo
        {
            var data = await _context.Dispatches.
                Include(o => o.CreatedByNavigation).
                Include(o => o.DispatchDetails)
                        .ThenInclude(od => od.StoreExportStoreDetails)
                        .ThenInclude(od => od.HandleByNavigation).
                Include(o => o.DispatchDetails)
                        .ThenInclude(od => od.Variant.Product).
                Include(o => o.DispatchDetails)
                        .ThenInclude(od => od.StoreExportStoreDetails)
                        .ThenInclude(od => od.Warehouse).
                Include(o => o.DispatchDetails)
                        .ThenInclude(od => od.StoreExportStoreDetails)
                        .ThenInclude(od => od.StaffDetail).
                FirstOrDefaultAsync(x => x.DispatchId == id);
            return data;
        }

        public async Task<StoreExportStoreDetail> GetStoreExportStoreDetailById(int importId)
        {
            var data = await _context.StoreExportStoreDetails
                           .Include(od => od.Warehouse)
                           .Include(od => od.StaffDetail).ThenInclude(oc => oc.Account)
                           .Include(od => od.HandleByNavigation.Account)
                           .Include(od => od.DispatchDetail)
                                   .ThenInclude(c => c.Dispatch).
                            Include(o => o.DispatchDetail)
                                    .ThenInclude(od => od.Variant.Product)
                           .FirstOrDefaultAsync(o => o.DispatchStoreDetailId == importId);
            return data;
        }
    }

}
