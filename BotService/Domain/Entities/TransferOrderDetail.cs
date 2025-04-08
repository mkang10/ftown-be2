using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class TransferOrderDetail
{
    public int TransferOrderDetailId { get; set; }

    public int TransferOrderId { get; set; }

    public int SourceStoreId { get; set; }

    public int ProductVariantId { get; set; }

    public int Quantity { get; set; }

    public int? DeliveredQuantity { get; set; }

    public virtual ProductVariantOffline ProductVariant { get; set; } = null!;

    public virtual StoreStock StoreStock { get; set; } = null!;

    public virtual TransferOrder TransferOrder { get; set; } = null!;
}
