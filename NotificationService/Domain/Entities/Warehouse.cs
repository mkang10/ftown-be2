using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Warehouse
{
    public int WarehouseId { get; set; }

    public string WarehouseName { get; set; } = null!;

    public string? WarehouseDescription { get; set; }

    public string Location { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public string? ImagePath { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string WarehouseType { get; set; } = null!;

    public virtual ICollection<CheckDetail> CheckDetails { get; set; } = new List<CheckDetail>();

    public virtual ICollection<ImportStoreDetail> ImportStoreDetails { get; set; } = new List<ImportStoreDetail>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<StoreExportStoreDetail> StoreExportStoreDetails { get; set; } = new List<StoreExportStoreDetail>();

    public virtual ICollection<WareHousesStock> WareHousesStocks { get; set; } = new List<WareHousesStock>();
}
