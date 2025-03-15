using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class StoreImport
{
    public int ImportId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? Status { get; set; }

    public string? ReferenceNumber { get; set; }

    public decimal? TotalCost { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public int? OriginalImportId { get; set; }

    public virtual Account CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<StoreImport> InverseOriginalImport { get; set; } = new List<StoreImport>();

    public virtual StoreImport? OriginalImport { get; set; }

    public virtual ICollection<StoreImportDetail> StoreImportDetails { get; set; } = new List<StoreImportDetail>();

    public virtual ICollection<StoreImportErrorReport> StoreImportErrorReports { get; set; } = new List<StoreImportErrorReport>();

    public virtual ICollection<StoreImportHistory> StoreImportHistories { get; set; } = new List<StoreImportHistory>();
}
