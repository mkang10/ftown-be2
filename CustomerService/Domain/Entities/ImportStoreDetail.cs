using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ImportStoreDetail
{
    public int ImportDetailId { get; set; }

    public int WareHouseId { get; set; }

    public int AllocatedQuantity { get; set; }

    public string? Status { get; set; }

    public string? Comments { get; set; }

    public int? StaffDetailId { get; set; }

    public int StoreImportStoreId { get; set; }

    public virtual ImportDetail ImportDetail { get; set; } = null!;

    public virtual Warehouse WareHouse { get; set; } = null!;
}
