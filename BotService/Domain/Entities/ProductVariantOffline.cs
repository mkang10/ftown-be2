using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ProductVariantOffline
{
    public int VariantOfflineId { get; set; }

    public int ProductId { get; set; }

    public string? Size { get; set; }

    public string? Color { get; set; }

    public decimal Price { get; set; }

    public string? ImagePath { get; set; }

    public string? Sku { get; set; }

    public string? Barcode { get; set; }

    public decimal? Weight { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<StoreImportDetail> StoreImportDetails { get; set; } = new List<StoreImportDetail>();

    public virtual ICollection<StoreStock> StoreStocks { get; set; } = new List<StoreStock>();

    public virtual ICollection<TransferOrderDetail> TransferOrderDetails { get; set; } = new List<TransferOrderDetail>();
}
