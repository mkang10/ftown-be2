using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly FtownContext _context;
        public WarehouseRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<Warehouse> CreateAsync(Warehouse warehouse)
        {
            await _context.Warehouses.AddAsync(warehouse);
            await _context.SaveChangesAsync();
            return warehouse;
        }
    }
}
