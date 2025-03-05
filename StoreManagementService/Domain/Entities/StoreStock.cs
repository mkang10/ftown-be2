using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class StoreStock
{
    public int StoreId { get; set; }

    public int VariantId { get; set; }

    public int StockQuantity { get; set; }

    public virtual Store Store { get; set; } = null!;

    public virtual ProductVariant Variant { get; set; } = null!;
}
