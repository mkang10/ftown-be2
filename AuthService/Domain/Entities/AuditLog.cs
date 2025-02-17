using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class AuditLog
{
    public int AuditLogId { get; set; }

    public string TableName { get; set; } = null!;

    public int RecordId { get; set; }

    public string ChangeType { get; set; } = null!;

    public int ChangedBy { get; set; }

    public string? ChangeData { get; set; }

    public DateTime? ChangedDate { get; set; }

    public virtual Account ChangedByNavigation { get; set; } = null!;
}
