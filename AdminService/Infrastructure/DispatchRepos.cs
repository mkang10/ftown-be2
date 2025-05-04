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
        public async Task<int> GetApprovedOutboundQuantityAsync(int warehouseId, int variantId)
        {
            // Dùng DispatchStoreDetails vì WarehouseId nằm ở đây
            var query = from dsd in _context.StoreExportStoreDetails
                        join dd in _context.DispatchDetails on dsd.DispatchDetailId equals dd.DispatchDetailId
                        join dj in _context.Dispatches on dd.DispatchId equals dj.DispatchId
                        where dsd.WarehouseId == warehouseId
                              && dd.VariantId == variantId
                              && dj.Status == "Approved"
                        select dsd.AllocatedQuantity;

            return await query.SumAsync(q => (int?)q) ?? 0;
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
                        .ThenInclude(od => od.HandleByNavigation).ThenInclude(od => od.Account).

                Include(o => o.DispatchDetails)
                        .ThenInclude(od => od.Variant.Product).
                Include(o => o.DispatchDetails)
                        .ThenInclude(od => od.StoreExportStoreDetails)
                        .ThenInclude(od => od.Warehouse)
                .Include(d => d.DispatchDetails)
                    .ThenInclude(dd => dd.Variant)
                        .ThenInclude(v => v.Color)
                .Include(d => d.DispatchDetails)
                    .ThenInclude(dd => dd.Variant)
                        .ThenInclude(v => v.Size).
                Include(o => o.DispatchDetails)
                        .ThenInclude(od => od.StoreExportStoreDetails)
                        .ThenInclude(od => od.StaffDetail).ThenInclude(od => od.Account).
                FirstOrDefaultAsync(x => x.DispatchId == id);
            return data;
        }

        public async Task ReloadAsync(Dispatch dispatch)
        {
            await _context.Entry(dispatch).ReloadAsync();
        }
        public async Task AddAsync(Dispatch dispatch)
           => await _context.Dispatches.AddAsync(dispatch);
       
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
                            .Include(o => o.DispatchDetail)
                                    .ThenInclude(od => od.Variant.Size)
                                     .Include(o => o.DispatchDetail)
                                    .ThenInclude(od => od.Variant.Color)
                           .FirstOrDefaultAsync(o => o.DispatchStoreDetailId == importId);
            return data;
        }
    }

}
