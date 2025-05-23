﻿using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class WareHousesStock
{
    public int WareHouseStockId { get; set; }

    public int VariantId { get; set; }

    public int StockQuantity { get; set; }

    public int WarehouseId { get; set; }

    public virtual ProductVariant Variant { get; set; } = null!;

    public virtual ICollection<WareHouseStockAudit> WareHouseStockAudits { get; set; } = new List<WareHouseStockAudit>();

    public virtual Warehouse Warehouse { get; set; } = null!;
}
