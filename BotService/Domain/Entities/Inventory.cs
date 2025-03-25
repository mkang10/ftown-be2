using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Inventory
{
    public int InventoryId { get; set; }

    public string InventoryName { get; set; } = null!;

    public string? Description { get; set; }

    public string? Location { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
}
