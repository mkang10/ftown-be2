using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class StoreCheckSession
{
    public int CheckSessionId { get; set; }

    public int OwnerId { get; set; }

    public DateTime SessionDate { get; set; }

    public string Status { get; set; } = null!;

    public string? Remarks { get; set; }

    public virtual Account Owner { get; set; } = null!;

    public virtual ICollection<StoreCheckDetail> StoreCheckDetails { get; set; } = new List<StoreCheckDetail>();

    public virtual ICollection<StoreCheckHistory> StoreCheckHistories { get; set; } = new List<StoreCheckHistory>();
}
