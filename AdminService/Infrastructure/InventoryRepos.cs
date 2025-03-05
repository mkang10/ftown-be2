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
    public class InventoryImportRepository : IInventoryImportRepository
    {
        private readonly FtownContext _context;

        public InventoryImportRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<InventoryImport> AddAsync(InventoryImport import)
        {
            await _context.InventoryImports.AddAsync(import);
            await _context.SaveChangesAsync();
            return import;
        }

        public async Task<InventoryImport?> GetByIdAsync(int importId)
        {
            return await _context.InventoryImports
                .Include(i => i.InventoryImportHistories)
                .FirstOrDefaultAsync(i => i.ImportId == importId);
        }

        public async Task UpdateAsync(InventoryImport import)
        {
            _context.InventoryImports.Update(import);
            await _context.SaveChangesAsync();
        }

        public async Task<Account?> GetAccountByIdAsync(int accountId)
        {
            return await _context.Accounts.FindAsync(accountId);
        }

        public async Task<List<InventoryImport>> GetAllPendingAsync()
        {
            return await _context.InventoryImports
                .Include(ii => ii.CreatedByNavigation)
                .Include(ii => ii.InventoryImportDetails)
                    .ThenInclude(detail => detail.InventoryImportStoreDetails)
                .Include(ii => ii.InventoryImportHistories)
                .Where(ii => ii.Status != null && ii.Status.ToLower() == "pending")
                .ToListAsync();
        }
    }
}
