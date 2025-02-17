using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class InventoryTransactionHistory
{
    public int InventoryTransactionHistoryId { get; set; }

    public int TransactionId { get; set; }

    public string Status { get; set; } = null!;

    public int ChangedBy { get; set; }

    public DateTime? ChangedDate { get; set; }

    public string? Comments { get; set; }

    public virtual Account ChangedByNavigation { get; set; } = null!;

    public virtual InventoryTransaction Transaction { get; set; } = null!;
}
