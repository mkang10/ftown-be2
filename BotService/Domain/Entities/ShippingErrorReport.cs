using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ShippingErrorReport
{
    public int ReportId { get; set; }

    public int TransferOrderId { get; set; }

    public int ReportedBy { get; set; }

    public string ReportDetails { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public string? Status { get; set; }

    public virtual Account ReportedByNavigation { get; set; } = null!;

    public virtual ICollection<ShippingErrorEscalation> ShippingErrorEscalations { get; set; } = new List<ShippingErrorEscalation>();

    public virtual TransferOrder TransferOrder { get; set; } = null!;
}
