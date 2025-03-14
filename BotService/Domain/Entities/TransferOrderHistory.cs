using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class TransferOrderHistory
{
    public int TransferOrderHistoryId { get; set; }

    public int TransferOrderId { get; set; }

    public string Status { get; set; } = null!;

    public int ChangedBy { get; set; }

    public DateTime? ChangedDate { get; set; }

    public string? Comments { get; set; }

    public virtual Account ChangedByNavigation { get; set; } = null!;

    public virtual TransferOrder TransferOrder { get; set; } = null!;
}
