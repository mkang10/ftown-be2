using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class OrderHistory
{
    public int OrderHistoryId { get; set; }

    public int OrderId { get; set; }

    public string OrderStatus { get; set; } = null!;

    public int ChangedBy { get; set; }

    public DateTime? ChangedDate { get; set; }

    public string? Comments { get; set; }

    public virtual Account ChangedByNavigation { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
