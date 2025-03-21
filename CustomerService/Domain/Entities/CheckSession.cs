using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class CheckSession
{
    public int CheckSessionId { get; set; }

    public int OwnerId { get; set; }

    public DateTime SessionDate { get; set; }

    public string Status { get; set; } = null!;

    public string? Remarks { get; set; }

    public virtual ICollection<CheckDetail> CheckDetails { get; set; } = new List<CheckDetail>();
}
