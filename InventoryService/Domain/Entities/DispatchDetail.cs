using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class DispatchDetail
{
    public int DispatchDetailId { get; set; }

    public int DispatchId { get; set; }

    public int ProductVariantOfflineId { get; set; }

    public int Quantity { get; set; }

    public virtual Dispatch Dispatch { get; set; } = null!;

    public virtual ICollection<StoreExportStoreDetail> StoreExportStoreDetails { get; set; } = new List<StoreExportStoreDetail>();
}
