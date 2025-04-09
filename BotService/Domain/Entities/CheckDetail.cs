using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class CheckDetail
{
    public int CheckDetailId { get; set; }

    public int CheckSessionId { get; set; }

    public int WarehouseId { get; set; }

    public int VariantOfflineId { get; set; }

    public int StaffId { get; set; }

    public int ExpectedQuantity { get; set; }

    public int CountedQuantity { get; set; }

    public int? Difference { get; set; }

    public string? Comments { get; set; }

    public int? ShopManagerId { get; set; }

    public virtual CheckSession CheckSession { get; set; } = null!;

    public virtual ShopManagerDetail? ShopManager { get; set; }

    public virtual StaffDetail Staff { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
