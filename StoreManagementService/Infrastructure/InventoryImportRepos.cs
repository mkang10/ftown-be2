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
    public class InventoryImportRepository : IInventoryImportRepos
    {
        private readonly FtownContext _context;

        public InventoryImportRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<InventoryImport> AddAsync(InventoryImport inventoryImport)
        {
            await _context.InventoryImports.AddAsync(inventoryImport);
            await _context.SaveChangesAsync();
            return inventoryImport;
        }

        public async Task<List<InventoryImport>> GetAllAsync(int createdby)
        {
            return await _context.InventoryImports.Where(ii => ii.CreatedBy == createdby)
            .Include(ii => ii.CreatedByNavigation).ToListAsync();
        }

       
    }
}
