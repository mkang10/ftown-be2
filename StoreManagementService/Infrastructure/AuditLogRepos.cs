using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class AuditLogRepos : IAuditLogRepos
    {
        private readonly FtownContext _context;

        public AuditLogRepos(FtownContext context)
        {
            _context = context;
        }

        public void Add(AuditLog auditLog)
        {
            _context.AuditLogs.Add(auditLog);
        }

      
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task AddAsync(AuditLog auditLog)
        {
            await _context.AuditLogs.AddAsync(auditLog);
        }
    }
}
