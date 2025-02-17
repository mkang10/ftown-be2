using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class InventoryTransaction
{
    public int TransactionId { get; set; }

    public int StoreId { get; set; }

    public string TransactionType { get; set; } = null!;

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? Status { get; set; }

    public string? ReferenceNumber { get; set; }

    public decimal? TransactionCost { get; set; }

    public virtual Account CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<InventoryTransactionDetail> InventoryTransactionDetails { get; set; } = new List<InventoryTransactionDetail>();

    public virtual ICollection<InventoryTransactionHistory> InventoryTransactionHistories { get; set; } = new List<InventoryTransactionHistory>();

    public virtual Store Store { get; set; } = null!;
}
