using System;
using System.Collections.Generic;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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

    public virtual DbSet<CheckDetail> CheckDetails { get; set; }

    public virtual DbSet<CheckSession> CheckSessions { get; set; }

    public virtual DbSet<Color> Colors { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<ConversationParticipant> ConversationParticipants { get; set; }

    public virtual DbSet<CustomerDetail> CustomerDetails { get; set; }

    public virtual DbSet<DeliveryTracking> DeliveryTrackings { get; set; }

    public virtual DbSet<Dispatch> Dispatches { get; set; }

    public virtual DbSet<DispatchDetail> DispatchDetails { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Import> Imports { get; set; }

    public virtual DbSet<ImportDetail> ImportDetails { get; set; }

    public virtual DbSet<ImportStoreDetail> ImportStoreDetails { get; set; }

    public virtual DbSet<Interest> Interests { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderAssignment> OrderAssignments { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<ProductVariant> ProductVariants { get; set; }

    public virtual DbSet<ReplyFeedback> ReplyFeedbacks { get; set; }

    public virtual DbSet<ReturnOrder> ReturnOrders { get; set; }

    public virtual DbSet<ReturnOrderItem> ReturnOrderItems { get; set; }

    public virtual DbSet<ReturnOrderMedium> ReturnOrderMedia { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<ShippingAddress> ShippingAddresses { get; set; }

    public virtual DbSet<ShopManagerDetail> ShopManagerDetails { get; set; }

    public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; }

    public virtual DbSet<Size> Sizes { get; set; }

    public virtual DbSet<StaffDetail> StaffDetails { get; set; }

    public virtual DbSet<StoreExportStoreDetail> StoreExportStoreDetails { get; set; }

    public virtual DbSet<Transfer> Transfers { get; set; }

    public virtual DbSet<TransferDetail> TransferDetails { get; set; }

    public virtual DbSet<WareHouseStockAudit> WareHouseStockAudits { get; set; }

    public virtual DbSet<WareHousesStock> WareHousesStocks { get; set; }

    public virtual DbSet<Warehouse> Warehouses { get; set; }

    public virtual DbSet<WishList> WishLists { get; set; }

    public virtual DbSet<WishListItem> WishListItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    
        => optionsBuilder.UseSqlServer("Server=funkytown.database.windows.net;Database=Ftown;User Id=funkytown;Password=Tuongvy2112;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Account__349DA5860B59E12A");

            entity.ToTable("Account");

            entity.HasIndex(e => e.Email, "IX_Account_Email");

            entity.HasIndex(e => e.Email, "UQ__Account__A9D105343D1979E1").IsUnique();

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
                .HasConstraintName("FK__Account__RoleID__2CF2ADDF");
        });

        modelBuilder.Entity<AccountInterest>(entity =>
        {
            entity.HasKey(e => e.AccountInterestId).HasName("PK__AccountI__E2B286B19E7C604F");

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
                .HasConstraintName("FK__AccountIn__Accou__2DE6D218");

            entity.HasOne(d => d.Interest).WithMany(p => p.AccountInterests)
                .HasForeignKey(d => d.InterestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AccountIn__Inter__2EDAF651");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId).HasName("PK__AuditLog__EB5F6CDD2D5AEE89");

            entity.ToTable("AuditLog");

            entity.Property(e => e.AuditLogId).HasColumnName("AuditLogID");
            entity.Property(e => e.ChangeDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Comment).HasMaxLength(100);
            entity.Property(e => e.Operation).HasMaxLength(50);
            entity.Property(e => e.RecordId)
                .HasMaxLength(100)
                .HasColumnName("RecordID");
            entity.Property(e => e.TableName).HasMaxLength(100);

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.ChangedBy)
                .HasConstraintName("FK_AuditLog_Account");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("PK__CartItem__488B0B2A30DF9933");

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
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A2BD93544D9");

            entity.ToTable("Category");

            entity.HasIndex(e => e.ParentCategoryId, "IX_Category_ParentCategoryID");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.ParentCategoryId).HasColumnName("ParentCategoryID");
        });

        modelBuilder.Entity<CheckDetail>(entity =>
        {
            entity.HasKey(e => e.CheckDetailId).HasName("PK__CheckDet__C0611DEAF551667D");

            entity.ToTable("CheckDetail");

            entity.Property(e => e.CheckDetailId).HasColumnName("CheckDetailID");
            entity.Property(e => e.CheckSessionId).HasColumnName("CheckSessionID");
            entity.Property(e => e.Comments).HasMaxLength(500);
            entity.Property(e => e.Difference).HasComputedColumnSql("([CountedQuantity]-[ExpectedQuantity])", false);
            entity.Property(e => e.ShopManagerId).HasColumnName("ShopManagerID");
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.WarehouseId).HasColumnName("WarehouseID");

            entity.HasOne(d => d.CheckSession).WithMany(p => p.CheckDetails)
                .HasForeignKey(d => d.CheckSessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StoreCheckDetail_StoreCheckSession");

            entity.HasOne(d => d.ShopManager).WithMany(p => p.CheckDetails)
                .HasForeignKey(d => d.ShopManagerId)
                .HasConstraintName("FK_StoreCheckDetail_ShopManagerDetail");

            entity.HasOne(d => d.Staff).WithMany(p => p.CheckDetails)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StoreCheckDetail_StaffDetail");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.CheckDetails)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StoreCheckDetail_Warehouses");
        });

        modelBuilder.Entity<CheckSession>(entity =>
        {
            entity.HasKey(e => e.CheckSessionId).HasName("PK__CheckSes__DA8D11523A7FE144");

            entity.ToTable("CheckSession");

            entity.Property(e => e.CheckSessionId).HasColumnName("CheckSessionID");
            entity.Property(e => e.OwnerId).HasColumnName("OwnerID");
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.SessionDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<Color>(entity =>
        {
            entity.HasKey(e => e.ColorId).HasName("PK__Color__8DA7676D900A225F");

            entity.ToTable("Color");

            entity.Property(e => e.ColorId).HasColumnName("ColorID");
            entity.Property(e => e.ColorCode).HasMaxLength(50);
            entity.Property(e => e.ColorName).HasMaxLength(50);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.ConversationId).HasName("PK__Conversa__C050D897A229792F");

            entity.ToTable("Conversation");

            entity.Property(e => e.ConversationId).HasColumnName("ConversationID");
            entity.Property(e => e.ConversationName).HasMaxLength(255);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<ConversationParticipant>(entity =>
        {
            entity.HasKey(e => new { e.ConversationId, e.AccountId }).HasName("PK__Conversa__B31902CFB2786781");

            entity.Property(e => e.ConversationId).HasColumnName("ConversationID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.JoinedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.ConversationParticipants)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ConversationParticipants_Account");

            entity.HasOne(d => d.Conversation).WithMany(p => p.ConversationParticipants)
                .HasForeignKey(d => d.ConversationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ConversationParticipants_Conversation");
        });

        modelBuilder.Entity<CustomerDetail>(entity =>
        {
            entity.HasKey(e => e.CustomerDetailId).HasName("PK__Customer__D04B36FEEC97A609");

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
                .HasConstraintName("FK__CustomerD__Accou__3864608B");
        });

        modelBuilder.Entity<DeliveryTracking>(entity =>
        {
            entity.HasKey(e => e.TrackingId).HasName("PK__Delivery__3C19EDD1EA74DB6D");

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
                .HasConstraintName("FK__DeliveryT__Order__395884C4");
        });

        modelBuilder.Entity<Dispatch>(entity =>
        {
            entity.HasKey(e => e.DispatchId).HasName("PK__Dispatch__434DBD75B1044AEC");

            entity.ToTable("Dispatch");

            entity.Property(e => e.DispatchId).HasColumnName("DispatchID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<DispatchDetail>(entity =>
        {
            entity.HasKey(e => e.DispatchDetailId).HasName("PK__Dispatch__8B84B60019484F53");

            entity.Property(e => e.DispatchDetailId).HasColumnName("DispatchDetailID");
            entity.Property(e => e.DispatchId).HasColumnName("DispatchID");
            entity.Property(e => e.ProductVariantOfflineId).HasColumnName("ProductVariantOfflineID");

            entity.HasOne(d => d.Dispatch).WithMany(p => p.DispatchDetails)
                .HasForeignKey(d => d.DispatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StoreDispatchDetails_StoreDispatch");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDF6B5E8AC50");

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
                .HasConstraintName("FK__Feedback__Accoun__3B40CD36");

            entity.HasOne(d => d.OrderDetail).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.OrderDetailId)
                .HasConstraintName("FK_Feedback_OrderDetails");

            entity.HasOne(d => d.Product).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__Produc__3C34F16F");
        });

        modelBuilder.Entity<Import>(entity =>
        {
            entity.HasKey(e => e.ImportId).HasName("PK__Import__8697678A5ACB46BC");

            entity.ToTable("Import");

            entity.Property(e => e.ImportId).HasColumnName("ImportID");
            entity.Property(e => e.ApprovedDate).HasColumnType("datetime");
            entity.Property(e => e.CompletedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.OriginalImportId).HasColumnName("OriginalImportID");
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TotalCost).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<ImportDetail>(entity =>
        {
            entity.HasKey(e => e.ImportDetailId).HasName("PK__ImportDe__CDFBBA51591B904E");

            entity.Property(e => e.ImportDetailId).HasColumnName("ImportDetailID");
            entity.Property(e => e.ImportId).HasColumnName("ImportID");
            entity.Property(e => e.ProductVariantOfflineId).HasColumnName("ProductVariantOfflineID");

            entity.HasOne(d => d.Import).WithMany(p => p.ImportDetails)
                .HasForeignKey(d => d.ImportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StoreImportDetails_StoreImport");
        });

        modelBuilder.Entity<ImportStoreDetail>(entity =>
        {
            entity.HasKey(e => e.StoreImportStoreId).HasName("PK_StoreImportStoreDetail");

            entity.ToTable("ImportStoreDetail");

            entity.Property(e => e.StoreImportStoreId)
                .ValueGeneratedNever()
                .HasColumnName("StoreImportStoreID");
            entity.Property(e => e.Comments).HasMaxLength(500);
            entity.Property(e => e.ImportDetailId).HasColumnName("ImportDetailID");
            entity.Property(e => e.StaffDetailId).HasColumnName("StaffDetailID");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.WareHouseId).HasColumnName("WareHouseID");

            entity.HasOne(d => d.ImportDetail).WithMany(p => p.ImportStoreDetails)
                .HasForeignKey(d => d.ImportDetailId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StoreImportStoreDetail_StoreImportDetails");

            entity.HasOne(d => d.WareHouse).WithMany(p => p.ImportStoreDetails)
                .HasForeignKey(d => d.WareHouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StoreImportStoreDetail_Warehouses");
        });

        modelBuilder.Entity<Interest>(entity =>
        {
            entity.HasKey(e => e.InterestId).HasName("PK__Interest__20832C0765CBD5B3");

            entity.Property(e => e.InterestId).HasColumnName("InterestID");
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Message__C87C037C396F02D1");

            entity.ToTable("Message");

            entity.Property(e => e.MessageId).HasColumnName("MessageID");
            entity.Property(e => e.ConversationId).HasColumnName("ConversationID");
            entity.Property(e => e.ParentMessageId).HasColumnName("ParentMessageID");
            entity.Property(e => e.SenderId).HasColumnName("SenderID");
            entity.Property(e => e.SentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ConversationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Message_Conversation");

            entity.HasOne(d => d.ParentMessage).WithMany(p => p.InverseParentMessage)
                .HasForeignKey(d => d.ParentMessageId)
                .HasConstraintName("FK_Message_Parent");

            entity.HasOne(d => d.Sender).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Message_Sender");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E327C8CF1B5");

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
                .HasConstraintName("FK__Notificat__Accou__42E1EEFE");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Order__C3905BAF9BE2CDA7");

            entity.ToTable("Order");

            entity.HasIndex(e => e.AccountId, "IX_Order_AccountID");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeliveryMethod).HasMaxLength(100);
            entity.Property(e => e.District).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.OrderTotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.Province).HasMaxLength(100);
            entity.Property(e => e.ShippingAddressId).HasColumnName("ShippingAddressID");
            entity.Property(e => e.ShippingCost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.WareHouseId).HasColumnName("WareHouseID");

            entity.HasOne(d => d.Account).WithMany(p => p.Orders)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__AccountID__43D61337");

            entity.HasOne(d => d.ShippingAddress).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ShippingAddressId)
                .HasConstraintName("FK__Order__ShippingA__44CA3770");

            entity.HasOne(d => d.WareHouse).WithMany(p => p.Orders)
                .HasForeignKey(d => d.WareHouseId)
                .HasConstraintName("FK_Order_Warehouses");
        });

        modelBuilder.Entity<OrderAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__OrderAss__32499E57873ACFC4");

            entity.ToTable("OrderAssignment");

            entity.Property(e => e.AssignmentId).HasColumnName("AssignmentID");
            entity.Property(e => e.AssignmentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Comments).HasMaxLength(500);
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ShopManagerId).HasColumnName("ShopManagerID");
            entity.Property(e => e.StaffId).HasColumnName("StaffID");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderAssignments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderAssignment_Order");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__OrderDet__D3B9D30CC2898CD2");

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
                .HasConstraintName("FK__OrderDeta__Order__47A6A41B");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductVariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__Produ__489AC854");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__9B556A5808E82BF9");

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
                .HasConstraintName("FK__Payment__OrderID__498EEC8D");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Product__B40CC6ED3E067758");

            entity.ToTable("Product");

            entity.HasIndex(e => e.CategoryId, "IX_Product_CategoryID");

            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.ColorId).HasColumnName("ColorID");
            entity.Property(e => e.Material).HasMaxLength(255);
            entity.Property(e => e.Model).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Occasion).HasMaxLength(255);
            entity.Property(e => e.Origin).HasMaxLength(255);
            entity.Property(e => e.SizeId).HasColumnName("SizeID");
            entity.Property(e => e.Style).HasMaxLength(255);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Product__Categor__4A8310C6");

            entity.HasOne(d => d.Color).WithMany(p => p.Products)
                .HasForeignKey(d => d.ColorId)
                .HasConstraintName("FK_Product_Color");

            entity.HasOne(d => d.Size).WithMany(p => p.Products)
                .HasForeignKey(d => d.SizeId)
                .HasConstraintName("FK_Product_Size");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.ProductImageId).HasName("PK__ProductI__07B2B1B8535AC6E4");

            entity.ToTable("ProductImage");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImagePath).HasMaxLength(255);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductImage_Product");
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(e => e.VariantId).HasName("PK__ProductV__0EA233E45714ADDD");

            entity.ToTable("ProductVariant");

            entity.HasIndex(e => e.ProductId, "IX_ProductVariant_ProductID");

            entity.HasIndex(e => e.Barcode, "UQ__ProductV__177800D36EC5A6D6").IsUnique();

            entity.HasIndex(e => e.Sku, "UQ__ProductV__CA1ECF0DB27DF675").IsUnique();

            entity.Property(e => e.VariantId).HasColumnName("VariantID");
            entity.Property(e => e.Barcode).HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.Size).HasMaxLength(50);
            entity.Property(e => e.Sku)
                .HasMaxLength(100)
                .HasColumnName("SKU");
            entity.Property(e => e.WarehouseStockId).HasColumnName("WarehouseStockID");
            entity.Property(e => e.Weight).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductVa__Produ__4E53A1AA");
        });

        modelBuilder.Entity<ReplyFeedback>(entity =>
        {
            entity.HasKey(e => e.ReplyId).HasName("PK__ReplyFee__C25E462989E55B4A");

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
                .HasConstraintName("FK__ReplyFeed__Accou__4F47C5E3");

            entity.HasOne(d => d.Feedback).WithMany(p => p.ReplyFeedbacks)
                .HasForeignKey(d => d.FeedbackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ReplyFeed__Feedb__503BEA1C");
        });

        modelBuilder.Entity<ReturnOrder>(entity =>
        {
            entity.HasKey(e => e.ReturnOrderId).HasName("PK__ReturnOr__4DBF5543DA69535E");

            entity.ToTable("ReturnOrder");

            entity.Property(e => e.BankAccountName).HasMaxLength(255);
            entity.Property(e => e.BankAccountNumber).HasMaxLength(50);
            entity.Property(e => e.BankName).HasMaxLength(255);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.RefundMethod)
                .HasMaxLength(50)
                .HasDefaultValue("Bank Transfer");
            entity.Property(e => e.ReturnOption).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TotalRefundAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.ReturnOrders)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_ReturnOrder_Order");
        });

        modelBuilder.Entity<ReturnOrderItem>(entity =>
        {
            entity.HasKey(e => e.ReturnOrderItemId).HasName("PK__ReturnOr__5F70CE663DA4ED5B");

            entity.ToTable("ReturnOrderItem");

            entity.Property(e => e.RefundPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.ReturnOrderItems)
                .HasForeignKey(d => d.ProductVariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReturnOrderItem_ProductVariant");

            entity.HasOne(d => d.ReturnOrder).WithMany(p => p.ReturnOrderItems)
                .HasForeignKey(d => d.ReturnOrderId)
                .HasConstraintName("FK_ReturnOrderItem_ReturnOrder");
        });

        modelBuilder.Entity<ReturnOrderMedium>(entity =>
        {
            entity.HasKey(e => e.ReturnOrderMediaId).HasName("PK__ReturnOr__8123C995ADE026E7");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.MediaUrl).HasMaxLength(2048);

            entity.HasOne(d => d.ReturnOrder).WithMany(p => p.ReturnOrderMedia)
                .HasForeignKey(d => d.ReturnOrderId)
                .HasConstraintName("FK_ReturnOrderMedia_ReturnOrder");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE3A35EF3CC7");

            entity.ToTable("Role");

            entity.HasIndex(e => e.RoleName, "UQ__Role__8A2B616087E33532").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RoleName).HasMaxLength(255);
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.SaleId).HasName("PK__Sale__1EE3C41F273F5168");

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
            entity.HasKey(e => e.AddressId).HasName("PK__Shipping__091C2A1BBFAFB8D0");

            entity.ToTable("ShippingAddress");

            entity.HasIndex(e => e.AccountId, "IX_ShippingAddress_AccountID");

            entity.Property(e => e.AddressId).HasColumnName("AddressID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.District).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Province).HasMaxLength(100);
            entity.Property(e => e.RecipientName)
                .HasMaxLength(255)
                .HasDefaultValue("");
            entity.Property(e => e.RecipientPhone)
                .HasMaxLength(20)
                .HasDefaultValue("");
            entity.Property(e => e.State).HasMaxLength(100);

            entity.HasOne(d => d.Account).WithMany(p => p.ShippingAddresses)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShippingA__Accou__55009F39");
        });

        modelBuilder.Entity<ShopManagerDetail>(entity =>
        {
            entity.HasKey(e => e.ShopManagerDetailId).HasName("PK__ShopMana__0E2E2C80DA734663");

            entity.ToTable("ShopManagerDetail");

            entity.HasIndex(e => e.StoreId, "UQ__ShopMana__3B82F0E012792B3E").IsUnique();

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
                .HasConstraintName("FK__ShopManag__Accou__55F4C372");
        });

        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__Shopping__51BCD7978C5C5D52");

            entity.ToTable("ShoppingCart");

            entity.Property(e => e.CartId).HasColumnName("CartID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.ShoppingCarts)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShoppingC__Accou__56E8E7AB");
        });

        modelBuilder.Entity<Size>(entity =>
        {
            entity.HasKey(e => e.SizeId).HasName("PK__Size__83BD095AEA83FD00");

            entity.ToTable("Size");

            entity.Property(e => e.SizeId).HasColumnName("SizeID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SizeDescription).HasMaxLength(255);
            entity.Property(e => e.SizeName).HasMaxLength(50);
        });

        modelBuilder.Entity<StaffDetail>(entity =>
        {
            entity.HasKey(e => e.StaffDetailId).HasName("PK__StaffDet__56818E83A81A8034");

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
                .HasConstraintName("FK__StaffDeta__Accou__57DD0BE4");
        });

        modelBuilder.Entity<StoreExportStoreDetail>(entity =>
        {
            entity.HasKey(e => e.DispatchStoreDetailId).HasName("PK_StoreExportStoreDetail_1");

            entity.ToTable("StoreExportStoreDetail");

            entity.Property(e => e.DispatchStoreDetailId)
                .ValueGeneratedNever()
                .HasColumnName("DispatchStoreDetailID");
            entity.Property(e => e.Comments).HasMaxLength(500);
            entity.Property(e => e.StaffDetailId).HasColumnName("StaffDetailID");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.WarehouseId).HasColumnName("WarehouseID");

            entity.HasOne(d => d.DispatchDetailNavigation).WithMany(p => p.StoreExportStoreDetails)
                .HasForeignKey(d => d.DispatchDetail)
                .HasConstraintName("FK_StoreExportStoreDetail_StoreDispatchDetails");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.StoreExportStoreDetails)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StoreExportStoreDetail_Warehouses");
        });

        modelBuilder.Entity<Transfer>(entity =>
        {
            entity.HasKey(e => e.TransferOrderId).HasName("PK__Transfer__4AEC45EE6B20CF2C");

            entity.ToTable("Transfer");

            entity.Property(e => e.TransferOrderId).HasColumnName("TransferOrderID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DispatchId).HasColumnName("DispatchID");
            entity.Property(e => e.ImportId).HasColumnName("ImportID");
            entity.Property(e => e.OriginalTransferOrderId).HasColumnName("OriginalTransferOrderID");
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Dispatch).WithMany(p => p.Transfers)
                .HasForeignKey(d => d.DispatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TransferOrder_StoreDispatch");

            entity.HasOne(d => d.Import).WithMany(p => p.Transfers)
                .HasForeignKey(d => d.ImportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TransferOrder_StoreImport");
        });

        modelBuilder.Entity<TransferDetail>(entity =>
        {
            entity.HasKey(e => e.TransferOrderDetailId).HasName("PK__Transfer__5BFCAC679CB54667");

            entity.Property(e => e.TransferOrderDetailId).HasColumnName("TransferOrderDetailID");
            entity.Property(e => e.ProductVariantOfflineId).HasColumnName("ProductVariantOfflineID");
            entity.Property(e => e.SourceStoreId).HasColumnName("SourceStoreID");
            entity.Property(e => e.TransferOrderId).HasColumnName("TransferOrderID");

            entity.HasOne(d => d.TransferOrder).WithMany(p => p.TransferDetails)
                .HasForeignKey(d => d.TransferOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TransferOrderDetails_TransferOrder");
        });

        modelBuilder.Entity<WareHouseStockAudit>(entity =>
        {
            entity.HasKey(e => e.AuditId).HasName("PK__WareHous__A17F23B869C31B42");

            entity.ToTable("WareHouseStockAudit");

            entity.Property(e => e.AuditId).HasColumnName("AuditID");
            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.ActionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.WareHouseStockId).HasColumnName("WareHouseStockID");

            entity.HasOne(d => d.WareHouseStock).WithMany(p => p.WareHouseStockAudits)
                .HasForeignKey(d => d.WareHouseStockId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WareHouseStockAudit_WareHousesStock");
        });

        modelBuilder.Entity<WareHousesStock>(entity =>
        {
            entity.HasKey(e => e.WareHouseStockId).HasName("PK__WareHous__ABE1832BB752DC29");

            entity.ToTable("WareHousesStock");

            entity.Property(e => e.WareHouseStockId).HasColumnName("WareHouseStockID");
            entity.Property(e => e.VariantId).HasColumnName("VariantID");
            entity.Property(e => e.WareHouseId).HasColumnName("WareHouseID");
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.WarehouseId).HasName("PK__Warehous__2608AFD9FDA6F7F6");

            entity.Property(e => e.WarehouseId).HasColumnName("WarehouseID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.WarehouseDescription).HasMaxLength(500);
            entity.Property(e => e.WarehouseName).HasMaxLength(255);
            entity.Property(e => e.WarehouseStockId).HasColumnName("WarehouseStockID");
            entity.Property(e => e.WarehouseType).HasMaxLength(50);

            entity.HasOne(d => d.WarehouseStock).WithMany(p => p.Warehouses)
                .HasForeignKey(d => d.WarehouseStockId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Warehouses_WareHousesStock");
        });

        modelBuilder.Entity<WishList>(entity =>
        {
            entity.HasKey(e => e.WishListId).HasName("PK__WishList__E41F87A7199F07EF");

            entity.ToTable("WishList");

            entity.Property(e => e.WishListId).HasColumnName("WishListID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.WishLists)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WishList__Accoun__5F7E2DAC");
        });

        modelBuilder.Entity<WishListItem>(entity =>
        {
            entity.HasKey(e => e.WishListItemId).HasName("PK__WishList__DAC208297F914903");

            entity.Property(e => e.WishListItemId).HasColumnName("WishListItemID");
            entity.Property(e => e.AddedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ProductVariantId).HasColumnName("ProductVariantID");
            entity.Property(e => e.WishListId).HasColumnName("WishListID");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.WishListItems)
                .HasForeignKey(d => d.ProductVariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WishListI__Produ__607251E5");

            entity.HasOne(d => d.WishList).WithMany(p => p.WishListItems)
                .HasForeignKey(d => d.WishListId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WishListI__WishL__6166761E");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
