using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class StoreCheckHistory
{
    public int CheckHistoryId { get; set; }

    public int CheckSessionId { get; set; }

    public string Action { get; set; } = null!;

    public int ChangedBy { get; set; }

    public DateTime ChangedDate { get; set; }

    public string? Comments { get; set; }

    public virtual Account ChangedByNavigation { get; set; } = null!;

    public virtual StoreCheckSession CheckSession { get; set; } = null!;
}
