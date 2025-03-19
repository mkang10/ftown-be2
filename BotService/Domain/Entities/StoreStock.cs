using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class StoreStock
{
    public int StoreId { get; set; }

    public int VariantOfflineId { get; set; }

    public int StockQuantity { get; set; }

    public virtual Store Store { get; set; } = null!;

    public virtual ICollection<StoreCheckDetail> StoreCheckDetails { get; set; } = new List<StoreCheckDetail>();

    public virtual ICollection<TransferOrderDetail> TransferOrderDetails { get; set; } = new List<TransferOrderDetail>();

    public virtual ProductVariantOffline VariantOffline { get; set; } = null!;
}
