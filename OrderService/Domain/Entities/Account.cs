using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Account
{
    public int AccountId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public bool? IsActive { get; set; }

    public int RoleId { get; set; }

    public string? ImagePath { get; set; }

    public virtual ICollection<AccountInterest> AccountInterests { get; set; } = new List<AccountInterest>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<CustomerDetail> CustomerDetails { get; set; } = new List<CustomerDetail>();

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<InventoryImportHistory> InventoryImportHistories { get; set; } = new List<InventoryImportHistory>();

    public virtual ICollection<InventoryImport> InventoryImports { get; set; } = new List<InventoryImport>();

    public virtual ICollection<InventoryTransactionHistory> InventoryTransactionHistories { get; set; } = new List<InventoryTransactionHistory>();

    public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<OrderHistory> OrderHistories { get; set; } = new List<OrderHistory>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<PaymentHistory> PaymentHistories { get; set; } = new List<PaymentHistory>();

    public virtual ICollection<ReplyFeedback> ReplyFeedbacks { get; set; } = new List<ReplyFeedback>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<ShippingAddress> ShippingAddresses { get; set; } = new List<ShippingAddress>();

    public virtual ICollection<ShopManagerDetail> ShopManagerDetails { get; set; } = new List<ShopManagerDetail>();

    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();

    public virtual ICollection<StaffDetail> StaffDetails { get; set; } = new List<StaffDetail>();

    public virtual ICollection<Store> Stores { get; set; } = new List<Store>();

    public virtual ICollection<WishList> WishLists { get; set; } = new List<WishList>();
}
