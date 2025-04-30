using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class WarehouseStockAuditRepository : IWarehouseStockAuditRepository
    {
        private readonly FtownContext _context;
        public WarehouseStockAuditRepository(FtownContext context) => _context = context;
        public async Task AddAsync(WareHouseStockAudit audit)
            => await _context.WareHouseStockAudits.AddAsync(audit);
    }
}
