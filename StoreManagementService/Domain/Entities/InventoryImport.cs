using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class InventoryImport
{
    public int ImportId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? Status { get; set; }

    public string? ReferenceNumber { get; set; }

    public decimal? TotalCost { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public virtual Account CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<InventoryImportDetail> InventoryImportDetails { get; set; } = new List<InventoryImportDetail>();

    public virtual ICollection<InventoryImportHistory> InventoryImportHistories { get; set; } = new List<InventoryImportHistory>();
}
