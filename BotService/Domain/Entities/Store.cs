using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Store
{
    public int StoreId { get; set; }

    public string StoreName { get; set; } = null!;

    public string? StoreDescription { get; set; }

    public string Location { get; set; } = null!;

    public int ManagerId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? ImagePath { get; set; }

    public string? StoreEmail { get; set; }

    public string? StorePhone { get; set; }

    public string? OperatingHours { get; set; }

    public virtual ICollection<InventoryImport> InventoryImports { get; set; } = new List<InventoryImport>();

    public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();

    public virtual Account Manager { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ShopManagerDetail? ShopManagerDetail { get; set; }

    public virtual ICollection<StaffDetail> StaffDetails { get; set; } = new List<StaffDetail>();
}
