using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IAuditLogRepos
    {
     
        Task AddAsync(AuditLog auditLog);

    
        Task SaveChangesAsync();

        Task<IEnumerable<AuditLog>> GetByRecordIdAsync(string tableName, string recordId);

    
        Task<IEnumerable<AuditLog>> GetByUserAsync(int changedBy);

        Task<List<AuditLog>> GetAuditLogsByTableAndRecordIdAsync(string tableName, string recordId);

        void Add(AuditLog auditLog);
    }
}

