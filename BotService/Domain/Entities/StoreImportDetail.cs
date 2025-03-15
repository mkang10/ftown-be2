using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class StoreImportDetail
{
    public int ImportDetailId { get; set; }

    public int ImportId { get; set; }

    public int ProductVariantOfflineId { get; set; }

    public int Quantity { get; set; }

    public virtual StoreImport Import { get; set; } = null!;

    public virtual ProductVariantOffline ProductVariantOffline { get; set; } = null!;

    public virtual ICollection<StoreImportStoreDetail> StoreImportStoreDetails { get; set; } = new List<StoreImportStoreDetail>();
}
