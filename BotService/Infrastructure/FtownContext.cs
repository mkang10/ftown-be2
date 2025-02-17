using System;
using System.Collections.Generic;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public partial class FtownContext : DbContext
{
    public FtownContext()
    {
    }

    public FtownContext(DbContextOptions<FtownContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AccountInterest> AccountInterests { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CustomerDetail> CustomerDetails { get; set; }

    public virtual DbSet<DeliveryTracking> DeliveryTrackings { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Interest> Interests { get; set; }

    public virtual DbSet<InventoryImport> InventoryImports { get; set; }

    public virtual DbSet<InventoryImportDetail> InventoryImportDetails { get; set; }

    public virtual DbSet<InventoryImportHistory> InventoryImportHistories { get; set; }

    public virtual DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    public virtual DbSet<InventoryTransactionDetail> InventoryTransactionDetails { get; set; }

    public virtual DbSet<InventoryTransactionHistory> InventoryTransactionHistories { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<OrderHistory> OrderHistories { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentHistory> PaymentHistories { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductVariant> ProductVariants { get; set; }

    public virtual DbSet<ReplyFeedback> ReplyFeedbacks { get; set; }

    public virtual DbSet<ReturnRequest> ReturnRequests { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<ShippingAddress> ShippingAddresses { get; set; }

    public virtual DbSet<ShopManagerDetail> ShopManagerDetails { get; set; }

    public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; }

    public virtual DbSet<StaffDetail> StaffDetails { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<WishList> WishLists { get; set; }

    public virtual DbSet<WishListItem> WishListItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-FEOOS2UC;Database=Ftown;Uid=sa;Pwd=123;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Account__349DA586D548610C");

            entity.ToTable("Account");

            entity.HasIndex(e => e.Email, "IX_Account_Email");

            entity.HasIndex(e => e.Email, "UQ__Account__A9D105342A5AF6C3").IsUnique();

            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastLoginDate).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Account__RoleID__3D5E1FD2");
        });

        modelBuilder.Entity<AccountInterest>(entity =>
        {
            entity.HasKey(e => e.AccountInterestId).HasName("PK__AccountI__E2B286B1B2E7D846");

            entity.Property(e => e.AccountInterestId).HasColumnName("AccountInterestID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.InteractionCount).HasDefaultValue(0);
            entity.Property(e => e.InterestId).HasColumnName("InterestID");
            entity.Property(e => e.LastInteractionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountInterests)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AccountIn__Accou__5BE2A6F2");

            entity.HasOne(d => d.Interest).WithMany(p => p.AccountInterests)
                .HasForeignKey(d => d.InterestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AccountIn__Inter__5CD6CB2B");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId).HasName("PK__AuditLog__EB5F6CDDC055512D");

            entity.ToTable("AuditLog");

            entity.Property(e => e.AuditLogId).HasColumnName("AuditLogID");
            entity.Property(e => e.ChangeType).HasMaxLength(50);
            entity.Property(e => e.ChangedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.TableName).HasMaxLength(255);

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.ChangedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AuditLog__Change__55009F39");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("PK__CartItem__488B0B2AEF1EF424");

            entity.Property(e => e.CartItemId).HasColumnName("CartItemID");
            entity.Property(e => e.CartId).HasColumnName("CartID");
            entity.Property(e => e.ProductVariantId).HasColumnName("ProductVariantID");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CartItems__CartI__30C33EC3");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductVariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CartItems__Produ__31B762FC");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A2B2D6E73FC");

            entity.ToTable("Category");

            entity.HasIndex(e => e.ParentCategoryId, "IX_Category_ParentCategoryID");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.ParentCategoryId).HasColumnName("ParentCategoryID");
        });

        modelBuilder.Entity<CustomerDetail>(entity =>
        {
            entity.HasKey(e => e.CustomerDetailId).HasName("PK__Customer__D04B36FE7788DF2B");

            entity.ToTable("CustomerDetail");

            entity.Property(e => e.CustomerDetailId).HasColumnName("CustomerDetailID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.CustomerType).HasMaxLength(50);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.LoyaltyPoints).HasDefaultValue(0);
            entity.Property(e => e.MembershipLevel)
                .HasMaxLength(50)
                .HasDefaultValue("Basic");
            entity.Property(e => e.PreferredPaymentMethod).HasMaxLength(50);

            entity.HasOne(d => d.Account).WithMany(p => p.CustomerDetails)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomerD__Accou__403A8C7D");
        });

        modelBuilder.Entity<DeliveryTracking>(entity =>
        {
            entity.HasKey(e => e.TrackingId).HasName("PK__Delivery__3C19EDD130AD1A9B");

            entity.ToTable("DeliveryTracking");

            entity.Property(e => e.TrackingId).HasColumnName("TrackingID");
            entity.Property(e => e.CurrentLocation).HasMaxLength(255);
            entity.Property(e => e.EstimatedDeliveryDate).HasColumnType("datetime");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("In Transit");

            entity.HasOne(d => d.Order).WithMany(p => p.DeliveryTrackings)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DeliveryT__Order__18EBB532");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__Document__1ABEEF6F257AC6FE");

            entity.ToTable("Document");

            entity.Property(e => e.DocumentId).HasColumnName("DocumentID");
            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
            entity.Property(e => e.UploadedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Documents)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Document__Transa__03F0984C");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.Documents)
                .HasForeignKey(d => d.UploadedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Document__Upload__04E4BC85");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDF680BC50EF");

            entity.ToTable("Feedback");

            entity.Property(e => e.FeedbackId).HasColumnName("FeedbackID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Account).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__Accoun__1DB06A4F");

            entity.HasOne(d => d.Product).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__Produc__1EA48E88");
        });

        modelBuilder.Entity<Interest>(entity =>
        {
            entity.HasKey(e => e.InterestId).HasName("PK__Interest__20832C071FA3114B");

            entity.Property(e => e.InterestId).HasColumnName("InterestID");
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<InventoryImport>(entity =>
        {
            entity.HasKey(e => e.ImportId).HasName("PK__Inventor__8697678AD99E65D3");

            entity.ToTable("InventoryImport");

            entity.Property(e => e.ImportId).HasColumnName("ImportID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.StoreId).HasColumnName("StoreID");
            entity.Property(e => e.TotalCost).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InventoryImports)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Creat__71D1E811");

            entity.HasOne(d => d.Store).WithMany(p => p.InventoryImports)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Store__70DDC3D8");
        });

        modelBuilder.Entity<InventoryImportDetail>(entity =>
        {
            entity.HasKey(e => e.ImportDetailId).HasName("PK__Inventor__CDFBBA51CB7EEB4C");

            entity.Property(e => e.ImportDetailId).HasColumnName("ImportDetailID");
            entity.Property(e => e.ImportId).HasColumnName("ImportID");
            entity.Property(e => e.ProductVariantId).HasColumnName("ProductVariantID");

            entity.HasOne(d => d.Import).WithMany(p => p.InventoryImportDetails)
                .HasForeignKey(d => d.ImportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Impor__76969D2E");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.InventoryImportDetails)
                .HasForeignKey(d => d.ProductVariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Produ__778AC167");
        });

        modelBuilder.Entity<InventoryImportHistory>(entity =>
        {
            entity.HasKey(e => e.InventoryImportHistoryId).HasName("PK__Inventor__D3F00254E3BF58F8");

            entity.ToTable("InventoryImportHistory");

            entity.Property(e => e.InventoryImportHistoryId).HasColumnName("InventoryImportHistoryID");
            entity.Property(e => e.ChangedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Comments).HasMaxLength(500);
            entity.Property(e => e.ImportId).HasColumnName("ImportID");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.InventoryImportHistories)
                .HasForeignKey(d => d.ChangedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Chang__51300E55");

            entity.HasOne(d => d.Import).WithMany(p => p.InventoryImportHistories)
                .HasForeignKey(d => d.ImportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Impor__503BEA1C");
        });

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Inventor__55433A4B265E0FDB");

            entity.ToTable("InventoryTransaction");

            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.StoreId).HasColumnName("StoreID");
            entity.Property(e => e.TransactionCost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TransactionType).HasMaxLength(50);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InventoryTransactions)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Creat__7B5B524B");

            entity.HasOne(d => d.Store).WithMany(p => p.InventoryTransactions)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Store__7A672E12");
        });

        modelBuilder.Entity<InventoryTransactionDetail>(entity =>
        {
            entity.HasKey(e => e.TransactionDetailId).HasName("PK__Inventor__F2B27FE64911344F");

            entity.Property(e => e.TransactionDetailId).HasColumnName("TransactionDetailID");
            entity.Property(e => e.ProductVariantId).HasColumnName("ProductVariantID");
            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.InventoryTransactionDetails)
                .HasForeignKey(d => d.ProductVariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Produ__01142BA1");

            entity.HasOne(d => d.Transaction).WithMany(p => p.InventoryTransactionDetails)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Trans__00200768");
        });

        modelBuilder.Entity<InventoryTransactionHistory>(entity =>
        {
            entity.HasKey(e => e.InventoryTransactionHistoryId).HasName("PK__Inventor__BD7D4BFEA4A0F4BA");

            entity.ToTable("InventoryTransactionHistory");

            entity.Property(e => e.InventoryTransactionHistoryId).HasColumnName("InventoryTransactionHistoryID");
            entity.Property(e => e.ChangedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Comments).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.InventoryTransactionHistories)
                .HasForeignKey(d => d.ChangedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Chang__4C6B5938");

            entity.HasOne(d => d.Transaction).WithMany(p => p.InventoryTransactionHistories)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Trans__4B7734FF");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E32B7E355CF");

            entity.ToTable("Notification");

            entity.Property(e => e.NotificationId).HasColumnName("NotificationID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.NotificationType).HasMaxLength(50);

            entity.HasOne(d => d.Account).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__Accou__282DF8C2");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Order__C3905BAF430106F1");

            entity.ToTable("Order", tb => tb.HasTrigger("trg_Order_Update"));

            entity.HasIndex(e => e.AccountId, "IX_Order_AccountID");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeliveryMethod).HasMaxLength(100);
            entity.Property(e => e.OrderTotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ShippingAddressId).HasColumnName("ShippingAddressID");
            entity.Property(e => e.ShippingCost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.StoreId).HasColumnName("StoreID");
            entity.Property(e => e.Tax).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Account).WithMany(p => p.Orders)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__AccountID__08B54D69");

            entity.HasOne(d => d.ShippingAddress).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ShippingAddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__ShippingA__0A9D95DB");

            entity.HasOne(d => d.Store).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__StoreID__09A971A2");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__OrderDet__D3B9D30C5010FA6E");

            entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");
            entity.Property(e => e.DiscountApplied)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.PriceAtPurchase).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductVariantId).HasColumnName("ProductVariantID");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__Order__0F624AF8");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductVariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__Produ__10566F31");
        });

        modelBuilder.Entity<OrderHistory>(entity =>
        {
            entity.HasKey(e => e.OrderHistoryId).HasName("PK__OrderHis__718E6CB3E822FEB7");

            entity.ToTable("OrderHistory");

            entity.Property(e => e.OrderHistoryId).HasColumnName("OrderHistoryID");
            entity.Property(e => e.ChangedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Comments).HasMaxLength(500);
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.OrderStatus).HasMaxLength(50);

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.OrderHistories)
                .HasForeignKey(d => d.ChangedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderHist__Chang__42E1EEFE");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderHistories)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderHist__Order__41EDCAC5");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__9B556A586DB16868");

            entity.ToTable("Payment");

            entity.HasIndex(e => e.OrderId, "IX_Payment_OrderID");

            entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.PaymentGatewayTransactionId)
                .HasMaxLength(100)
                .HasColumnName("PaymentGatewayTransactionID");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PaymentReference).HasMaxLength(100);
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payment__OrderID__14270015");
        });

        modelBuilder.Entity<PaymentHistory>(entity =>
        {
            entity.HasKey(e => e.PaymentHistoryId).HasName("PK__PaymentH__F3B93391F3014DBC");

            entity.ToTable("PaymentHistory");

            entity.Property(e => e.PaymentHistoryId).HasColumnName("PaymentHistoryID");
            entity.Property(e => e.ChangedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Comments).HasMaxLength(500);
            entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
            entity.Property(e => e.PaymentStatus).HasMaxLength(50);

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.PaymentHistories)
                .HasForeignKey(d => d.ChangedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PaymentHi__Chang__47A6A41B");

            entity.HasOne(d => d.Payment).WithMany(p => p.PaymentHistories)
                .HasForeignKey(d => d.PaymentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PaymentHi__Payme__46B27FE2");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Product__B40CC6EDF867D9BF");

            entity.ToTable("Product");

            entity.HasIndex(e => e.CategoryId, "IX_Product_CategoryID");

            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.Material).HasMaxLength(255);
            entity.Property(e => e.Model).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Occasion).HasMaxLength(255);
            entity.Property(e => e.Origin).HasMaxLength(255);
            entity.Property(e => e.Style).HasMaxLength(255);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Product__Categor__4D94879B");
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(e => e.VariantId).HasName("PK__ProductV__0EA233E4BC510934");

            entity.ToTable("ProductVariant");

            entity.HasIndex(e => e.ProductId, "IX_ProductVariant_ProductID");

            entity.HasIndex(e => e.Barcode, "UQ__ProductV__177800D32CF66DFF").IsUnique();

            entity.HasIndex(e => e.Sku, "UQ__ProductV__CA1ECF0DD85975C9").IsUnique();

            entity.Property(e => e.VariantId).HasColumnName("VariantID");
            entity.Property(e => e.Barcode).HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.Size).HasMaxLength(50);
            entity.Property(e => e.Sku)
                .HasMaxLength(100)
                .HasColumnName("SKU");
            entity.Property(e => e.StockQuantity).HasDefaultValue(0);
            entity.Property(e => e.Weight).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductVa__Produ__52593CB8");
        });

        modelBuilder.Entity<ReplyFeedback>(entity =>
        {
            entity.HasKey(e => e.ReplyId).HasName("PK__ReplyFee__C25E4629833B99DE");

            entity.ToTable("ReplyFeedback");

            entity.Property(e => e.ReplyId).HasColumnName("ReplyID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FeedbackId).HasColumnName("FeedbackID");

            entity.HasOne(d => d.Account).WithMany(p => p.ReplyFeedbacks)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ReplyFeed__Accou__245D67DE");

            entity.HasOne(d => d.Feedback).WithMany(p => p.ReplyFeedbacks)
                .HasForeignKey(d => d.FeedbackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ReplyFeed__Feedb__236943A5");
        });

        modelBuilder.Entity<ReturnRequest>(entity =>
        {
            entity.HasKey(e => e.ReturnRequestId).HasName("PK__ReturnRe__0CCD25B9DA3C863B");

            entity.ToTable("ReturnRequest");

            entity.Property(e => e.ReturnRequestId).HasColumnName("ReturnRequestID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Order).WithMany(p => p.ReturnRequests)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ReturnReq__Order__3D2915A8");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE3AC0E5974F");

            entity.ToTable("Role");

            entity.HasIndex(e => e.RoleName, "UQ__Role__8A2B6160EF9D30BB").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RoleName).HasMaxLength(255);
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.SaleId).HasName("PK__Sale__1EE3C41FB23CCCD8");

            entity.ToTable("Sale");

            entity.Property(e => e.SaleId).HasColumnName("SaleID");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DiscountRate).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SaleName).HasMaxLength(255);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<ShippingAddress>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__Shipping__091C2A1B824B8FFA");

            entity.ToTable("ShippingAddress");

            entity.HasIndex(e => e.AccountId, "IX_ShippingAddress_AccountID");

            entity.Property(e => e.AddressId).HasColumnName("AddressID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.State).HasMaxLength(100);

            entity.HasOne(d => d.Account).WithMany(p => p.ShippingAddresses)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShippingA__Accou__44FF419A");
        });

        modelBuilder.Entity<ShopManagerDetail>(entity =>
        {
            entity.HasKey(e => e.ShopManagerDetailId).HasName("PK__ShopMana__0E2E2C80C284B0A7");

            entity.ToTable("ShopManagerDetail");

            entity.HasIndex(e => e.StoreId, "UQ__ShopMana__3B82F0E09C31713E").IsUnique();

            entity.Property(e => e.ShopManagerDetailId).HasColumnName("ShopManagerDetailID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.ManagedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ManagerCertifications).HasMaxLength(255);
            entity.Property(e => e.OfficeContact).HasMaxLength(50);
            entity.Property(e => e.StoreId).HasColumnName("StoreID");

            entity.HasOne(d => d.Account).WithMany(p => p.ShopManagerDetails)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShopManag__Accou__6754599E");

            entity.HasOne(d => d.Store).WithOne(p => p.ShopManagerDetail)
                .HasForeignKey<ShopManagerDetail>(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShopManag__Store__68487DD7");
        });

        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__Shopping__51BCD797418F530C");

            entity.ToTable("ShoppingCart");

            entity.Property(e => e.CartId).HasColumnName("CartID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.ShoppingCarts)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShoppingC__Accou__2CF2ADDF");
        });

        modelBuilder.Entity<StaffDetail>(entity =>
        {
            entity.HasKey(e => e.StaffDetailId).HasName("PK__StaffDet__56818E833C7D6EF2");

            entity.ToTable("StaffDetail");

            entity.Property(e => e.StaffDetailId).HasColumnName("StaffDetailID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.EmploymentType).HasMaxLength(50);
            entity.Property(e => e.JobTitle).HasMaxLength(100);
            entity.Property(e => e.JoinDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Role).HasMaxLength(255);
            entity.Property(e => e.Salary).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.StoreId).HasColumnName("StoreID");

            entity.HasOne(d => d.Account).WithMany(p => p.StaffDetails)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StaffDeta__Accou__6C190EBB");

            entity.HasOne(d => d.Store).WithMany(p => p.StaffDetails)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StaffDeta__Store__6D0D32F4");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.StoreId).HasName("PK__Store__3B82F0E1C22ADEBD");

            entity.ToTable("Store");

            entity.HasIndex(e => e.ManagerId, "UQ__Store__3BA2AA80267A8482").IsUnique();

            entity.Property(e => e.StoreId).HasColumnName("StoreID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.ManagerId).HasColumnName("ManagerID");
            entity.Property(e => e.OperatingHours).HasMaxLength(100);
            entity.Property(e => e.StoreDescription).HasMaxLength(500);
            entity.Property(e => e.StoreEmail).HasMaxLength(255);
            entity.Property(e => e.StoreName).HasMaxLength(255);
            entity.Property(e => e.StorePhone).HasMaxLength(50);

            entity.HasOne(d => d.Manager).WithOne(p => p.Store)
                .HasForeignKey<Store>(d => d.ManagerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Store__ManagerID__628FA481");
        });

        modelBuilder.Entity<WishList>(entity =>
        {
            entity.HasKey(e => e.WishListId).HasName("PK__WishList__E41F87A7BD63E8A3");

            entity.ToTable("WishList");

            entity.Property(e => e.WishListId).HasColumnName("WishListID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.WishLists)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WishList__Accoun__3493CFA7");
        });

        modelBuilder.Entity<WishListItem>(entity =>
        {
            entity.HasKey(e => e.WishListItemId).HasName("PK__WishList__DAC208291AAF54E5");

            entity.Property(e => e.WishListItemId).HasColumnName("WishListItemID");
            entity.Property(e => e.AddedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ProductVariantId).HasColumnName("ProductVariantID");
            entity.Property(e => e.WishListId).HasColumnName("WishListID");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.WishListItems)
                .HasForeignKey(d => d.ProductVariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WishListI__Produ__395884C4");

            entity.HasOne(d => d.WishList).WithMany(p => p.WishListItems)
                .HasForeignKey(d => d.WishListId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WishListI__WishL__3864608B");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
