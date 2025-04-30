using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IAuditLogRepos
    {
        void Add(AuditLog auditLog);
        Task SaveChangesAsync();
        Task AddAsync(AuditLog auditLog);

    }
}
