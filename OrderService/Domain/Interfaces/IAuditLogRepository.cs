using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IAuditLogRepository
    {
        Task AddAuditLogAsync(string tableName, string recordId, string operation, int changedBy, string? changeData, string? comment);
        Task<Dictionary<int, DateTime>> GetLatestDeliveredDatesAsync(List<int> orderIds);
    }

}
