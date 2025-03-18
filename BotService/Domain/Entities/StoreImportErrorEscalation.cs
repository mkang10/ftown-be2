using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class StoreImportErrorEscalation
{
    public int EscalationId { get; set; }

    public int ReportId { get; set; }

    public int ShopManagerId { get; set; }

    public string EscalationDetails { get; set; } = null!;

    public DateTime? EscalationDate { get; set; }

    public string? OwnerApprovalStatus { get; set; }

    public int? OwnerId { get; set; }

    public DateTime? OwnerApprovalDate { get; set; }

    public string? OwnerComments { get; set; }

    public virtual Account? Owner { get; set; }

    public virtual StoreImportErrorReport Report { get; set; } = null!;

    public virtual Account ShopManager { get; set; } = null!;
}
