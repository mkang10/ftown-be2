using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class InventoryImportStoreDetail
{
    public int ImportDetailId { get; set; }

    public int StoreId { get; set; }

    public int AllocatedQuantity { get; set; }

    public string? Status { get; set; }

    public string? Comments { get; set; }

    public int? StaffDetailId { get; set; }

    public virtual InventoryImportDetail ImportDetail { get; set; } = null!;

    public virtual StaffDetail? StaffDetail { get; set; }

    public virtual Store Store { get; set; } = null!;
}
