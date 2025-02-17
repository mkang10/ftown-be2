using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class InventoryTransactionDetail
{
    public int TransactionDetailId { get; set; }

    public int TransactionId { get; set; }

    public int ProductVariantId { get; set; }

    public int Quantity { get; set; }

    public virtual ProductVariant ProductVariant { get; set; } = null!;

    public virtual InventoryTransaction Transaction { get; set; } = null!;
}
