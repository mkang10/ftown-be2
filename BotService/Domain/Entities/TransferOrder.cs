using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class TransferOrder
{
    public int TransferOrderId { get; set; }

    public int SourceStoreId { get; set; }

    public int DestinationStoreId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string Status { get; set; } = null!;

    public string? Remarks { get; set; }

    public int? OriginalTransferOrderId { get; set; }

    public virtual Account CreatedByNavigation { get; set; } = null!;

    public virtual Store DestinationStore { get; set; } = null!;

    public virtual ICollection<TransferOrder> InverseOriginalTransferOrder { get; set; } = new List<TransferOrder>();

    public virtual TransferOrder? OriginalTransferOrder { get; set; }

    public virtual ICollection<ShippingErrorReport> ShippingErrorReports { get; set; } = new List<ShippingErrorReport>();

    public virtual Store SourceStore { get; set; } = null!;

    public virtual ICollection<TransferOrderDetail> TransferOrderDetails { get; set; } = new List<TransferOrderDetail>();

    public virtual ICollection<TransferOrderHistory> TransferOrderHistories { get; set; } = new List<TransferOrderHistory>();
}
