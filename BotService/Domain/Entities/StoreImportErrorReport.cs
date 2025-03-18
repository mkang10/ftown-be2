using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class StoreImportErrorReport
{
    public int ReportId { get; set; }

    public int ImportId { get; set; }

    public int ReportedBy { get; set; }

    public string ErrorType { get; set; } = null!;

    public string ReportDetails { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public string? Status { get; set; }

    public virtual StoreImport Import { get; set; } = null!;

    public virtual Account ReportedByNavigation { get; set; } = null!;

    public virtual ICollection<StoreImportErrorEscalation> StoreImportErrorEscalations { get; set; } = new List<StoreImportErrorEscalation>();
}
