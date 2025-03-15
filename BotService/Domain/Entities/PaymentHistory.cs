using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class PaymentHistory
{
    public int PaymentHistoryId { get; set; }

    public int PaymentId { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public int ChangedBy { get; set; }

    public DateTime? ChangedDate { get; set; }

    public string? Comments { get; set; }

    public virtual Account ChangedByNavigation { get; set; } = null!;

    public virtual Payment Payment { get; set; } = null!;
}
