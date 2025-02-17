using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class InventoryImportDetail
{
    public int ImportDetailId { get; set; }

    public int ImportId { get; set; }

    public int ProductVariantId { get; set; }

    public int Quantity { get; set; }

    public virtual InventoryImport Import { get; set; } = null!;

    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
