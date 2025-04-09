using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class AuditLogHandler
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogHandler(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task LogOrderStatusChangeAsync(int orderId, string newStatus, int changedBy, string comment)
        {
            await _auditLogRepository.AddAuditLogAsync(
                "Orders",
                orderId.ToString(),
                newStatus,
                changedBy,
                null,
                comment
            );
        }

        public async Task LogOrderStatusChangeWithDetailsAsync(int orderId, string oldStatus, string newStatus, int changedBy, string comment)
        {
            var changeData = new
            {
                OldStatus = oldStatus,
                NewStatus = newStatus
            };

            await _auditLogRepository.AddAuditLogAsync(
                "Orders",
                orderId.ToString(),
                newStatus,
                changedBy,
                JsonSerializer.Serialize(changeData),
                comment
            );
        }
    }

}
