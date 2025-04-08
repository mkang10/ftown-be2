using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class StoreCheckDetail
{
    public int CheckDetailId { get; set; }

    public int CheckSessionId { get; set; }

    public int StoreId { get; set; }

    public int VariantOfflineId { get; set; }

    public int StaffId { get; set; }

    public int ExpectedQuantity { get; set; }

    public int CountedQuantity { get; set; }

    public int? Difference { get; set; }

    public string? Comments { get; set; }

    public virtual StoreCheckSession CheckSession { get; set; } = null!;

    public virtual StaffDetail Staff { get; set; } = null!;

    public virtual Store Store { get; set; } = null!;

    public virtual StoreStock StoreStock { get; set; } = null!;
}
