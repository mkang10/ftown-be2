using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class StoreExportStoreDetail
{
    public int DispatchStoreDetailId { get; set; }

    public int WarehouseId { get; set; }

    public int AllocatedQuantity { get; set; }

    public string? Status { get; set; }

    public string? Comments { get; set; }

    public int? StaffDetailId { get; set; }

    public int? DispatchDetail { get; set; }

    public virtual DispatchDetail? DispatchDetailNavigation { get; set; }

    public virtual Warehouse Warehouse { get; set; } = null!;
}
