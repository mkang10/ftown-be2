using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class InventoryImportHistory
{
    public int InventoryImportHistoryId { get; set; }

    public int ImportId { get; set; }

    public string Status { get; set; } = null!;

    public int ChangedBy { get; set; }

    public DateTime? ChangedDate { get; set; }

    public string? Comments { get; set; }

    public virtual Account? ChangedByNavigation { get; set; } = null!;

    public virtual InventoryImport Import { get; set; } = null!;
}
