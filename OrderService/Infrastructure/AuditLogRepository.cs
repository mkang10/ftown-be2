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
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly FtownContext _context;

        public AuditLogRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task AddAuditLogAsync(string tableName, string recordId, string operation, int changedBy, string? changeData, string? comment)
        {
            var auditLog = new AuditLog
            {
                TableName = tableName,
                RecordId = recordId,
                Operation = operation,
                ChangeDate = DateTime.UtcNow,
                ChangedBy = changedBy,
                ChangeData = changeData,
                Comment = comment
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
        public async Task<Dictionary<int, DateTime>> GetLatestDeliveredDatesAsync(List<int> orderIds)
        {
            return await _context.AuditLogs
                .Where(al => al.TableName == "Orders" && al.Operation == "delivered" && orderIds.Contains(int.Parse(al.RecordId)))
                .GroupBy(al => al.RecordId)
                .Select(g => new
                {
                    OrderId = int.Parse(g.Key),
                    DeliveredDate = g.Max(al => al.ChangeDate)
                })
                .ToDictionaryAsync(x => x.OrderId, x => x.DeliveredDate);
        }
    }
}
