using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class GHNLogRepository : IGHNLogRepository
    {
        private readonly FtownContext _context;

        public GHNLogRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<AuditLog> CreateAuditLog(AuditLog data)
        {
            _context.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }
    }
}
