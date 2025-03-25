using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ImportStoreDetail
{
    public int WareHouseId { get; set; }

    public int AllocatedQuantity { get; set; }

    public string? Status { get; set; }

    public string? Comments { get; set; }

    public int? StaffDetailId { get; set; }

    public int ImportDetailId { get; set; }

    public int ImportStoreId { get; set; }

    public virtual ImportDetail ImportDetail { get; set; } = null!;

    public virtual StaffDetail? StaffDetail { get; set; }

    public virtual Warehouse WareHouse { get; set; } = null!;
}
