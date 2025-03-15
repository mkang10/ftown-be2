using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ReturnOrderHistory
{
    public int ReturnOrderHistoryId { get; set; }

    public int ReturnOrderId { get; set; }

    public string Status { get; set; } = null!;

    public string ChangedBy { get; set; } = null!;

    public DateTime ChangedDate { get; set; }

    public string? Comments { get; set; }

    public virtual ReturnOrder ReturnOrder { get; set; } = null!;
}
