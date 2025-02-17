using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Order
{
    public int OrderId { get; set; }

    public int AccountId { get; set; }

    public int StoreId { get; set; }

    public int ShippingAddressId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? Status { get; set; }

    public decimal? OrderTotal { get; set; }

    public decimal? ShippingCost { get; set; }

    public decimal? Tax { get; set; }

    public string? DeliveryMethod { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<DeliveryTracking> DeliveryTrackings { get; set; } = new List<DeliveryTracking>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<OrderHistory> OrderHistories { get; set; } = new List<OrderHistory>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<ReturnRequest> ReturnRequests { get; set; } = new List<ReturnRequest>();

    public virtual ShippingAddress ShippingAddress { get; set; } = null!;

    public virtual Store Store { get; set; } = null!;
}
