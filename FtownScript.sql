USE [master]
GO
/****** Object:  Database [Ftown]    Script Date: 2/13/2025 2:43:32 PM ******/
CREATE DATABASE [Ftown]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Ftown', FILENAME = N'E:\ssms\DataSSMS\Ftown.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Ftown_log', FILENAME = N'E:\ssms\LogSSMS\Ftown_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [Ftown] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Ftown].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Ftown] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Ftown] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Ftown] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Ftown] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Ftown] SET ARITHABORT OFF 
GO
ALTER DATABASE [Ftown] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [Ftown] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Ftown] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Ftown] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Ftown] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Ftown] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Ftown] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Ftown] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Ftown] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Ftown] SET  DISABLE_BROKER 
GO
ALTER DATABASE [Ftown] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Ftown] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Ftown] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Ftown] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Ftown] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Ftown] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [Ftown] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Ftown] SET RECOVERY FULL 
GO
ALTER DATABASE [Ftown] SET  MULTI_USER 
GO
ALTER DATABASE [Ftown] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Ftown] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Ftown] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Ftown] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [Ftown] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [Ftown] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'Ftown', N'ON'
GO
ALTER DATABASE [Ftown] SET QUERY_STORE = ON
GO
ALTER DATABASE [Ftown] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [Ftown]
GO
/****** Object:  Table [dbo].[Account]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Account](
	[AccountID] [int] IDENTITY(1,1) NOT NULL,
	[FullName] [nvarchar](255) NOT NULL,
	[Email] [nvarchar](255) NOT NULL,
	[PasswordHash] [nvarchar](255) NOT NULL,
	[PhoneNumber] [nvarchar](15) NULL,
	[Address] [nvarchar](255) NULL,
	[CreatedDate] [datetime] NULL,
	[LastLoginDate] [datetime] NULL,
	[IsActive] [bit] NULL,
	[RoleID] [int] NOT NULL,
	[ImagePath] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[AccountID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AccountInterests]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccountInterests](
	[AccountInterestID] [int] IDENTITY(1,1) NOT NULL,
	[AccountID] [int] NOT NULL,
	[InterestID] [int] NOT NULL,
	[InteractionCount] [int] NULL,
	[LastInteractionDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[AccountInterestID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AuditLog]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AuditLog](
	[AuditLogID] [int] IDENTITY(1,1) NOT NULL,
	[TableName] [nvarchar](255) NOT NULL,
	[RecordID] [int] NOT NULL,
	[ChangeType] [nvarchar](50) NOT NULL,
	[ChangedBy] [int] NOT NULL,
	[ChangeData] [nvarchar](max) NULL,
	[ChangedDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[AuditLogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CartItems]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CartItems](
	[CartItemID] [int] IDENTITY(1,1) NOT NULL,
	[CartID] [int] NOT NULL,
	[ProductVariantID] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[CartItemID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Category]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Category](
	[CategoryID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[ParentCategoryID] [int] NULL,
	[IsActive] [bit] NULL,
	[DisplayOrder] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[CategoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CustomerDetail]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerDetail](
	[CustomerDetailID] [int] IDENTITY(1,1) NOT NULL,
	[AccountID] [int] NOT NULL,
	[LoyaltyPoints] [int] NULL,
	[MembershipLevel] [nvarchar](50) NULL,
	[DateOfBirth] [date] NULL,
	[Gender] [nvarchar](10) NULL,
	[CustomerType] [nvarchar](50) NULL,
	[PreferredPaymentMethod] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[CustomerDetailID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DeliveryTracking]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DeliveryTracking](
	[TrackingID] [int] IDENTITY(1,1) NOT NULL,
	[OrderID] [int] NOT NULL,
	[CurrentLocation] [nvarchar](255) NULL,
	[Status] [nvarchar](50) NULL,
	[LastUpdated] [datetime] NULL,
	[EstimatedDeliveryDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[TrackingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Document]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Document](
	[DocumentID] [int] IDENTITY(1,1) NOT NULL,
	[TransactionID] [int] NOT NULL,
	[UploadedBy] [int] NOT NULL,
	[UploadedDate] [datetime] NULL,
	[FilePath] [nvarchar](max) NULL,
	[ImagePath] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[DocumentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Feedback]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Feedback](
	[FeedbackID] [int] IDENTITY(1,1) NOT NULL,
	[AccountID] [int] NOT NULL,
	[ProductID] [int] NOT NULL,
	[Title] [nvarchar](255) NULL,
	[Rating] [int] NULL,
	[Comment] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NULL,
	[ImagePath] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[FeedbackID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Interests]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Interests](
	[InterestID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[InterestID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InventoryImport]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InventoryImport](
	[ImportID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [int] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedDate] [datetime] NULL,
	[Status] [nvarchar](50) NULL,
	[ReferenceNumber] [nvarchar](100) NULL,
	[TotalCost] [decimal](10, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[ImportID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InventoryImportDetails]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InventoryImportDetails](
	[ImportDetailID] [int] IDENTITY(1,1) NOT NULL,
	[ImportID] [int] NOT NULL,
	[ProductVariantID] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ImportDetailID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InventoryImportHistory]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InventoryImportHistory](
	[InventoryImportHistoryID] [int] IDENTITY(1,1) NOT NULL,
	[ImportID] [int] NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[ChangedBy] [int] NOT NULL,
	[ChangedDate] [datetime] NULL,
	[Comments] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[InventoryImportHistoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InventoryTransaction]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InventoryTransaction](
	[TransactionID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [int] NOT NULL,
	[TransactionType] [nvarchar](50) NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedDate] [datetime] NULL,
	[Status] [nvarchar](50) NULL,
	[ReferenceNumber] [nvarchar](100) NULL,
	[TransactionCost] [decimal](10, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[TransactionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InventoryTransactionDetails]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InventoryTransactionDetails](
	[TransactionDetailID] [int] IDENTITY(1,1) NOT NULL,
	[TransactionID] [int] NOT NULL,
	[ProductVariantID] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[TransactionDetailID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InventoryTransactionHistory]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InventoryTransactionHistory](
	[InventoryTransactionHistoryID] [int] IDENTITY(1,1) NOT NULL,
	[TransactionID] [int] NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[ChangedBy] [int] NOT NULL,
	[ChangedDate] [datetime] NULL,
	[Comments] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[InventoryTransactionHistoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Notification]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notification](
	[NotificationID] [int] IDENTITY(1,1) NOT NULL,
	[AccountID] [int] NOT NULL,
	[Content] [nvarchar](max) NULL,
	[NotificationType] [nvarchar](50) NULL,
	[IsRead] [bit] NULL,
	[CreatedDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[NotificationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Order]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Order](
	[OrderID] [int] IDENTITY(1,1) NOT NULL,
	[AccountID] [int] NOT NULL,
	[StoreID] [int] NOT NULL,
	[ShippingAddressID] [int] NOT NULL,
	[CreatedDate] [datetime] NULL,
	[Status] [nvarchar](50) NULL,
	[OrderTotal] [decimal](10, 2) NULL,
	[ShippingCost] [decimal](10, 2) NULL,
	[Tax] [decimal](10, 2) NULL,
	[DeliveryMethod] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[OrderID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderDetails]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderDetails](
	[OrderDetailID] [int] IDENTITY(1,1) NOT NULL,
	[OrderID] [int] NOT NULL,
	[ProductVariantID] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[PriceAtPurchase] [decimal](10, 2) NOT NULL,
	[DiscountApplied] [decimal](10, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[OrderDetailID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderHistory]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderHistory](
	[OrderHistoryID] [int] IDENTITY(1,1) NOT NULL,
	[OrderID] [int] NOT NULL,
	[OrderStatus] [nvarchar](50) NOT NULL,
	[ChangedBy] [int] NOT NULL,
	[ChangedDate] [datetime] NULL,
	[Comments] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[OrderHistoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Payment]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Payment](
	[PaymentID] [int] IDENTITY(1,1) NOT NULL,
	[OrderID] [int] NOT NULL,
	[PaymentMethod] [nvarchar](50) NOT NULL,
	[PaymentStatus] [nvarchar](50) NULL,
	[TransactionDate] [datetime] NULL,
	[Amount] [decimal](10, 2) NOT NULL,
	[PaymentReference] [nvarchar](100) NULL,
	[PaymentGatewayTransactionID] [nvarchar](100) NULL,
	[PaymentNotes] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[PaymentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PaymentHistory]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentHistory](
	[PaymentHistoryID] [int] IDENTITY(1,1) NOT NULL,
	[PaymentID] [int] NOT NULL,
	[PaymentStatus] [nvarchar](50) NOT NULL,
	[ChangedBy] [int] NOT NULL,
	[ChangedDate] [datetime] NULL,
	[Comments] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[PaymentHistoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Product]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Product](
	[ProductID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CategoryID] [int] NULL,
	[ImagePath] [nvarchar](max) NULL,
	[Origin] [nvarchar](255) NULL,
	[Model] [nvarchar](255) NULL,
	[Occasion] [nvarchar](255) NULL,
	[Style] [nvarchar](255) NULL,
	[Material] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[ProductID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductVariant]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductVariant](
	[VariantID] [int] IDENTITY(1,1) NOT NULL,
	[ProductID] [int] NOT NULL,
	[Size] [nvarchar](50) NULL,
	[Color] [nvarchar](50) NULL,
	[Price] [decimal](10, 2) NOT NULL,
	[StockQuantity] [int] NULL,
	[ImagePath] [nvarchar](max) NULL,
	[SKU] [nvarchar](100) NULL,
	[Barcode] [nvarchar](100) NULL,
	[Weight] [decimal](10, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[VariantID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Barcode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[SKU] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReplyFeedback]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReplyFeedback](
	[ReplyID] [int] IDENTITY(1,1) NOT NULL,
	[FeedbackID] [int] NOT NULL,
	[AccountID] [int] NOT NULL,
	[ReplyText] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ReplyID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReturnRequest]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReturnRequest](
	[ReturnRequestID] [int] IDENTITY(1,1) NOT NULL,
	[OrderID] [int] NOT NULL,
	[Reason] [nvarchar](500) NOT NULL,
	[Status] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[UpdatedDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ReturnRequestID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Role]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Role](
	[RoleID] [int] IDENTITY(1,1) NOT NULL,
	[RoleName] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[RoleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[RoleName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Sale]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sale](
	[SaleID] [int] IDENTITY(1,1) NOT NULL,
	[SaleName] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[DiscountRate] [decimal](5, 2) NULL,
	[IsActive] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[SaleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShippingAddress]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShippingAddress](
	[AddressID] [int] IDENTITY(1,1) NOT NULL,
	[AccountID] [int] NOT NULL,
	[Address] [nvarchar](500) NOT NULL,
	[City] [nvarchar](100) NULL,
	[State] [nvarchar](100) NULL,
	[Country] [nvarchar](100) NULL,
	[PostalCode] [nvarchar](20) NULL,
	[IsDefault] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[AddressID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShopManagerDetail]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShopManagerDetail](
	[ShopManagerDetailID] [int] IDENTITY(1,1) NOT NULL,
	[AccountID] [int] NOT NULL,
	[StoreID] [int] NOT NULL,
	[ManagedDate] [datetime] NULL,
	[YearsOfExperience] [int] NULL,
	[ManagerCertifications] [nvarchar](255) NULL,
	[OfficeContact] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[ShopManagerDetailID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[StoreID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShoppingCart]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShoppingCart](
	[CartID] [int] IDENTITY(1,1) NOT NULL,
	[AccountID] [int] NOT NULL,
	[CreatedDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[CartID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StaffDetail]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StaffDetail](
	[StaffDetailID] [int] IDENTITY(1,1) NOT NULL,
	[AccountID] [int] NOT NULL,
	[StoreID] [int] NOT NULL,
	[JoinDate] [datetime] NULL,
	[Role] [nvarchar](255) NOT NULL,
	[JobTitle] [nvarchar](100) NOT NULL,
	[Department] [nvarchar](100) NOT NULL,
	[Salary] [decimal](10, 2) NULL,
	[EmploymentType] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[StaffDetailID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Store]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Store](
	[StoreID] [int] IDENTITY(1,1) NOT NULL,
	[StoreName] [nvarchar](255) NOT NULL,
	[StoreDescription] [nvarchar](500) NULL,
	[Location] [nvarchar](255) NOT NULL,
	[ManagerID] [int] NOT NULL,
	[CreatedDate] [datetime] NULL,
	[ImagePath] [nvarchar](max) NULL,
	[StoreEmail] [nvarchar](255) NULL,
	[StorePhone] [nvarchar](50) NULL,
	[OperatingHours] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[StoreID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[ManagerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WishList]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WishList](
	[WishListID] [int] IDENTITY(1,1) NOT NULL,
	[AccountID] [int] NOT NULL,
	[CreatedDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[WishListID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WishListItems]    Script Date: 2/13/2025 2:43:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WishListItems](
	[WishListItemID] [int] IDENTITY(1,1) NOT NULL,
	[WishListID] [int] NOT NULL,
	[ProductVariantID] [int] NOT NULL,
	[AddedDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[WishListItemID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Account_Email]    Script Date: 2/13/2025 2:43:32 PM ******/
CREATE NONCLUSTERED INDEX [IX_Account_Email] ON [dbo].[Account]
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Category_ParentCategoryID]    Script Date: 2/13/2025 2:43:32 PM ******/
CREATE NONCLUSTERED INDEX [IX_Category_ParentCategoryID] ON [dbo].[Category]
(
	[ParentCategoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Order_AccountID]    Script Date: 2/13/2025 2:43:32 PM ******/
CREATE NONCLUSTERED INDEX [IX_Order_AccountID] ON [dbo].[Order]
(
	[AccountID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Payment_OrderID]    Script Date: 2/13/2025 2:43:32 PM ******/
CREATE NONCLUSTERED INDEX [IX_Payment_OrderID] ON [dbo].[Payment]
(
	[OrderID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Product_CategoryID]    Script Date: 2/13/2025 2:43:32 PM ******/
CREATE NONCLUSTERED INDEX [IX_Product_CategoryID] ON [dbo].[Product]
(
	[CategoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductVariant_ProductID]    Script Date: 2/13/2025 2:43:32 PM ******/
CREATE NONCLUSTERED INDEX [IX_ProductVariant_ProductID] ON [dbo].[ProductVariant]
(
	[ProductID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ShippingAddress_AccountID]    Script Date: 2/13/2025 2:43:32 PM ******/
CREATE NONCLUSTERED INDEX [IX_ShippingAddress_AccountID] ON [dbo].[ShippingAddress]
(
	[AccountID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Account] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Account] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[AccountInterests] ADD  DEFAULT ((0)) FOR [InteractionCount]
GO
ALTER TABLE [dbo].[AccountInterests] ADD  DEFAULT (getdate()) FOR [LastInteractionDate]
GO
ALTER TABLE [dbo].[AuditLog] ADD  DEFAULT (getdate()) FOR [ChangedDate]
GO
ALTER TABLE [dbo].[Category] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Category] ADD  DEFAULT ((0)) FOR [DisplayOrder]
GO
ALTER TABLE [dbo].[CustomerDetail] ADD  DEFAULT ((0)) FOR [LoyaltyPoints]
GO
ALTER TABLE [dbo].[CustomerDetail] ADD  DEFAULT ('Basic') FOR [MembershipLevel]
GO
ALTER TABLE [dbo].[DeliveryTracking] ADD  DEFAULT ('In Transit') FOR [Status]
GO
ALTER TABLE [dbo].[DeliveryTracking] ADD  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Document] ADD  DEFAULT (getdate()) FOR [UploadedDate]
GO
ALTER TABLE [dbo].[Feedback] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[InventoryImport] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[InventoryImport] ADD  DEFAULT ('Pending') FOR [Status]
GO
ALTER TABLE [dbo].[InventoryImportHistory] ADD  DEFAULT (getdate()) FOR [ChangedDate]
GO
ALTER TABLE [dbo].[InventoryTransaction] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[InventoryTransaction] ADD  DEFAULT ('Pending') FOR [Status]
GO
ALTER TABLE [dbo].[InventoryTransactionHistory] ADD  DEFAULT (getdate()) FOR [ChangedDate]
GO
ALTER TABLE [dbo].[Notification] ADD  DEFAULT ((0)) FOR [IsRead]
GO
ALTER TABLE [dbo].[Notification] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Order] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Order] ADD  DEFAULT ('Pending') FOR [Status]
GO
ALTER TABLE [dbo].[OrderDetails] ADD  DEFAULT ((0)) FOR [DiscountApplied]
GO
ALTER TABLE [dbo].[OrderHistory] ADD  DEFAULT (getdate()) FOR [ChangedDate]
GO
ALTER TABLE [dbo].[Payment] ADD  DEFAULT ('Pending') FOR [PaymentStatus]
GO
ALTER TABLE [dbo].[Payment] ADD  DEFAULT (getdate()) FOR [TransactionDate]
GO
ALTER TABLE [dbo].[PaymentHistory] ADD  DEFAULT (getdate()) FOR [ChangedDate]
GO
ALTER TABLE [dbo].[ProductVariant] ADD  DEFAULT ((0)) FOR [StockQuantity]
GO
ALTER TABLE [dbo].[ReplyFeedback] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[ReturnRequest] ADD  DEFAULT ('Pending') FOR [Status]
GO
ALTER TABLE [dbo].[ReturnRequest] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Sale] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[ShippingAddress] ADD  DEFAULT ((0)) FOR [IsDefault]
GO
ALTER TABLE [dbo].[ShopManagerDetail] ADD  DEFAULT (getdate()) FOR [ManagedDate]
GO
ALTER TABLE [dbo].[ShoppingCart] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[StaffDetail] ADD  DEFAULT (getdate()) FOR [JoinDate]
GO
ALTER TABLE [dbo].[Store] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[WishList] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[WishListItems] ADD  DEFAULT (getdate()) FOR [AddedDate]
GO
ALTER TABLE [dbo].[Account]  WITH CHECK ADD FOREIGN KEY([RoleID])
REFERENCES [dbo].[Role] ([RoleID])
GO
ALTER TABLE [dbo].[AccountInterests]  WITH CHECK ADD FOREIGN KEY([AccountID])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[AccountInterests]  WITH CHECK ADD FOREIGN KEY([InterestID])
REFERENCES [dbo].[Interests] ([InterestID])
GO
ALTER TABLE [dbo].[AuditLog]  WITH CHECK ADD FOREIGN KEY([ChangedBy])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[CartItems]  WITH CHECK ADD FOREIGN KEY([CartID])
REFERENCES [dbo].[ShoppingCart] ([CartID])
GO
ALTER TABLE [dbo].[CartItems]  WITH CHECK ADD FOREIGN KEY([ProductVariantID])
REFERENCES [dbo].[ProductVariant] ([VariantID])
GO
ALTER TABLE [dbo].[CustomerDetail]  WITH CHECK ADD FOREIGN KEY([AccountID])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[DeliveryTracking]  WITH CHECK ADD FOREIGN KEY([OrderID])
REFERENCES [dbo].[Order] ([OrderID])
GO
ALTER TABLE [dbo].[Document]  WITH CHECK ADD FOREIGN KEY([TransactionID])
REFERENCES [dbo].[InventoryTransaction] ([TransactionID])
GO
ALTER TABLE [dbo].[Document]  WITH CHECK ADD FOREIGN KEY([UploadedBy])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[Feedback]  WITH CHECK ADD FOREIGN KEY([AccountID])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[Feedback]  WITH CHECK ADD FOREIGN KEY([ProductID])
REFERENCES [dbo].[Product] ([ProductID])
GO
ALTER TABLE [dbo].[InventoryImport]  WITH CHECK ADD FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[InventoryImport]  WITH CHECK ADD FOREIGN KEY([StoreID])
REFERENCES [dbo].[Store] ([StoreID])
GO
ALTER TABLE [dbo].[InventoryImportDetails]  WITH CHECK ADD FOREIGN KEY([ImportID])
REFERENCES [dbo].[InventoryImport] ([ImportID])
GO
ALTER TABLE [dbo].[InventoryImportDetails]  WITH CHECK ADD FOREIGN KEY([ProductVariantID])
REFERENCES [dbo].[ProductVariant] ([VariantID])
GO
ALTER TABLE [dbo].[InventoryImportHistory]  WITH CHECK ADD FOREIGN KEY([ChangedBy])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[InventoryImportHistory]  WITH CHECK ADD FOREIGN KEY([ImportID])
REFERENCES [dbo].[InventoryImport] ([ImportID])
GO
ALTER TABLE [dbo].[InventoryTransaction]  WITH CHECK ADD FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[InventoryTransaction]  WITH CHECK ADD FOREIGN KEY([StoreID])
REFERENCES [dbo].[Store] ([StoreID])
GO
ALTER TABLE [dbo].[InventoryTransactionDetails]  WITH CHECK ADD FOREIGN KEY([ProductVariantID])
REFERENCES [dbo].[ProductVariant] ([VariantID])
GO
ALTER TABLE [dbo].[InventoryTransactionDetails]  WITH CHECK ADD FOREIGN KEY([TransactionID])
REFERENCES [dbo].[InventoryTransaction] ([TransactionID])
GO
ALTER TABLE [dbo].[InventoryTransactionHistory]  WITH CHECK ADD FOREIGN KEY([ChangedBy])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[InventoryTransactionHistory]  WITH CHECK ADD FOREIGN KEY([TransactionID])
REFERENCES [dbo].[InventoryTransaction] ([TransactionID])
GO
ALTER TABLE [dbo].[Notification]  WITH CHECK ADD FOREIGN KEY([AccountID])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[Order]  WITH CHECK ADD FOREIGN KEY([AccountID])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[Order]  WITH CHECK ADD FOREIGN KEY([ShippingAddressID])
REFERENCES [dbo].[ShippingAddress] ([AddressID])
GO
ALTER TABLE [dbo].[Order]  WITH CHECK ADD FOREIGN KEY([StoreID])
REFERENCES [dbo].[Store] ([StoreID])
GO
ALTER TABLE [dbo].[OrderDetails]  WITH CHECK ADD FOREIGN KEY([OrderID])
REFERENCES [dbo].[Order] ([OrderID])
GO
ALTER TABLE [dbo].[OrderDetails]  WITH CHECK ADD FOREIGN KEY([ProductVariantID])
REFERENCES [dbo].[ProductVariant] ([VariantID])
GO
ALTER TABLE [dbo].[OrderHistory]  WITH CHECK ADD FOREIGN KEY([ChangedBy])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[OrderHistory]  WITH CHECK ADD FOREIGN KEY([OrderID])
REFERENCES [dbo].[Order] ([OrderID])
GO
ALTER TABLE [dbo].[Payment]  WITH CHECK ADD FOREIGN KEY([OrderID])
REFERENCES [dbo].[Order] ([OrderID])
GO
ALTER TABLE [dbo].[PaymentHistory]  WITH CHECK ADD FOREIGN KEY([ChangedBy])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[PaymentHistory]  WITH CHECK ADD FOREIGN KEY([PaymentID])
REFERENCES [dbo].[Payment] ([PaymentID])
GO
ALTER TABLE [dbo].[Product]  WITH CHECK ADD FOREIGN KEY([CategoryID])
REFERENCES [dbo].[Category] ([CategoryID])
GO
ALTER TABLE [dbo].[ProductVariant]  WITH CHECK ADD FOREIGN KEY([ProductID])
REFERENCES [dbo].[Product] ([ProductID])
GO
ALTER TABLE [dbo].[ReplyFeedback]  WITH CHECK ADD FOREIGN KEY([AccountID])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[ReplyFeedback]  WITH CHECK ADD FOREIGN KEY([FeedbackID])
REFERENCES [dbo].[Feedback] ([FeedbackID])
GO
ALTER TABLE [dbo].[ReturnRequest]  WITH CHECK ADD FOREIGN KEY([OrderID])
REFERENCES [dbo].[Order] ([OrderID])
GO
ALTER TABLE [dbo].[ShippingAddress]  WITH CHECK ADD FOREIGN KEY([AccountID])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[ShopManagerDetail]  WITH CHECK ADD FOREIGN KEY([AccountID])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[ShopManagerDetail]  WITH CHECK ADD FOREIGN KEY([StoreID])
REFERENCES [dbo].[Store] ([StoreID])
GO
ALTER TABLE [dbo].[ShoppingCart]  WITH CHECK ADD FOREIGN KEY([AccountID])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[StaffDetail]  WITH CHECK ADD FOREIGN KEY([AccountID])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[StaffDetail]  WITH CHECK ADD FOREIGN KEY([StoreID])
REFERENCES [dbo].[Store] ([StoreID])
GO
ALTER TABLE [dbo].[Store]  WITH CHECK ADD FOREIGN KEY([ManagerID])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[WishList]  WITH CHECK ADD FOREIGN KEY([AccountID])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[WishListItems]  WITH CHECK ADD FOREIGN KEY([ProductVariantID])
REFERENCES [dbo].[ProductVariant] ([VariantID])
GO
ALTER TABLE [dbo].[WishListItems]  WITH CHECK ADD FOREIGN KEY([WishListID])
REFERENCES [dbo].[WishList] ([WishListID])
GO
ALTER TABLE [dbo].[Category]  WITH CHECK ADD  CONSTRAINT [CHK_Category_NoSelfReference] CHECK  (([ParentCategoryID] IS NULL OR [ParentCategoryID]<>[CategoryID]))
GO
ALTER TABLE [dbo].[Category] CHECK CONSTRAINT [CHK_Category_NoSelfReference]
GO
ALTER TABLE [dbo].[Feedback]  WITH CHECK ADD CHECK  (([Rating]>=(1) AND [Rating]<=(5)))
GO
ALTER TABLE [dbo].[Sale]  WITH CHECK ADD CHECK  (([DiscountRate]>=(0) AND [DiscountRate]<=(100)))
GO
USE [master]
GO
ALTER DATABASE [Ftown] SET  READ_WRITE 
GO

ALTER TABLE [dbo].[Order]
ADD 
    [FullName] NVARCHAR(255) NOT NULL,  -- Tên người nhận hàng
    [Email] NVARCHAR(255) NOT NULL,  -- Email liên hệ
    [PhoneNumber] NVARCHAR(15) NOT NULL,  -- Số điện thoại liên hệ
    [Address] NVARCHAR(500) NOT NULL,  -- Địa chỉ giao hàng
    [City] NVARCHAR(100) NOT NULL,  -- Thành phố
    [District] NVARCHAR(100) NOT NULL;  -- Quận/Huyện

ALTER TABLE [dbo].[Order] ALTER COLUMN ShippingAddressId INT NULL;
ALTER TABLE [dbo].[Order] ALTER COLUMN StoreID INT NULL;


CREATE TABLE [dbo].[StoreStock] (
    [StoreId] INT NOT NULL,
    [VariantId] INT NOT NULL,
    [StockQuantity] INT NOT NULL,
    CONSTRAINT [PK_StoreStock] PRIMARY KEY ([StoreId], [VariantId]),
    CONSTRAINT [FK_StoreStock_Store] FOREIGN KEY ([StoreId]) REFERENCES [dbo].[Store]([StoreId]),
    CONSTRAINT [FK_StoreStock_ProductVariant] FOREIGN KEY ([VariantId]) REFERENCES [dbo].[ProductVariant]([VariantId])
);



ALTER TABLE ProductVariant 
DROP CONSTRAINT DF__ProductVa__Stock__2B0A656D;

ALTER TABLE ProductVariant 
DROP COLUMN StockQuantity;

-- Bật IDENTITY_INSERT để chèn ID thủ công vào Product
SET IDENTITY_INSERT Product ON;
INSERT INTO Product (ProductID, Name, Description, CategoryID, ImagePath, Origin, Model, Occasion, Style, Material)
VALUES 
    (1, 'Áo Thun', 'Áo thun cotton thoáng mát', 1, 'image1.jpg', 'Vietnam', 'Basic', 'Casual', 'Modern', 'Cotton'),
    (2, 'Áo Sơ Mi', 'Áo sơ mi công sở', 1, 'image2.jpg', 'Vietnam', 'Office', 'Formal', 'Classic', 'Polyester'),
    (3, 'Quần Jean', 'Quần jean nam nữ', 2, 'image3.jpg', 'Vietnam', 'Slim Fit', 'Casual', 'Trendy', 'Denim'),
    (4, 'Quần Short', 'Quần short thoải mái', 2, 'image4.jpg', 'Vietnam', 'Loose Fit', 'Casual', 'Modern', 'Cotton'),
    (5, 'Váy', 'Váy công sở nữ', 3, 'image5.jpg', 'Vietnam', 'A-Line', 'Formal', 'Elegant', 'Silk'),
    (6, 'Hoodie', 'Áo hoodie trẻ trung', 1, 'image6.jpg', 'Vietnam', 'Oversized', 'Casual', 'Streetwear', 'Fleece'),
    (7, 'Jacket', 'Áo khoác giữ ấm', 1, 'image7.jpg', 'Vietnam', 'Windbreaker', 'Casual', 'Sporty', 'Polyester');
SET IDENTITY_INSERT Product OFF;

-- Bật IDENTITY_INSERT để chèn ID thủ công vào ProductVariant
SET IDENTITY_INSERT ProductVariant ON;
INSERT INTO ProductVariant (VariantID, ProductID, Size, Color, Price, ImagePath, SKU, Barcode, Weight)
VALUES 
    (1, 1, 'M', 'Đen', 200000, 'variant1.jpg', 'AT-M-BK', '1234561', 0.3),
    (2, 1, 'L', 'Trắng', 220000, 'variant2.jpg', 'AT-L-WH', '1234562', 0.3),
    (3, 1, 'XL', 'Xanh', 250000, 'variant3.jpg', 'AT-XL-BL', '1234563', 0.3),
    
    (4, 2, 'M', 'Xanh', 300000, 'variant4.jpg', 'ASM-M-BL', '1234564', 0.4),
    (5, 2, 'L', 'Trắng', 320000, 'variant5.jpg', 'ASM-L-WH', '1234565', 0.4),
    (6, 2, 'XL', 'Đen', 350000, 'variant6.jpg', 'ASM-XL-BK', '1234566', 0.4),
    
    (7, 3, '30', 'Xanh', 500000, 'variant7.jpg', 'QJ-30-BL', '1234567', 0.7),
    (8, 3, '32', 'Đen', 520000, 'variant8.jpg', 'QJ-32-BK', '1234568', 0.7),
    (9, 3, '34', 'Xám', 550000, 'variant9.jpg', 'QJ-34-GY', '1234569', 0.7),
    
    (10, 4, 'M', 'Đỏ', 250000, 'variant10.jpg', 'QS-M-RD', '1234570', 0.4),
    (11, 4, 'L', 'Xanh', 270000, 'variant11.jpg', 'QS-L-BL', '1234571', 0.4),
    (12, 4, 'XL', 'Vàng', 300000, 'variant12.jpg', 'QS-XL-YL', '1234572', 0.4),
    
    (13, 5, 'S', 'Hồng', 400000, 'variant13.jpg', 'VAY-S-PK', '1234573', 0.3),
    (14, 5, 'M', 'Xanh', 420000, 'variant14.jpg', 'VAY-M-BL', '1234574', 0.3),
    (15, 5, 'L', 'Đỏ', 450000, 'variant15.jpg', 'VAY-L-RD', '1234575', 0.3),
    
    (16, 6, 'M', 'Xám', 600000, 'variant16.jpg', 'HD-M-GY', '1234576', 0.6),
    (17, 6, 'L', 'Đen', 620000, 'variant17.jpg', 'HD-L-BK', '1234577', 0.6),
    (18, 6, 'XL', 'Xanh', 650000, 'variant18.jpg', 'HD-XL-BL', '1234578', 0.6),
    
    (19, 7, 'M', 'Đen', 700000, 'variant19.jpg', 'JK-M-BK', '1234579', 0.8),
    (20, 7, 'L', 'Xanh', 720000, 'variant20.jpg', 'JK-L-BL', '1234580', 0.8),
    (21, 7, 'XL', 'Xám', 750000, 'variant21.jpg', 'JK-XL-GY', '1234581', 0.8);
SET IDENTITY_INSERT ProductVariant OFF;

-- Chèn dữ liệu vào bảng Store (không cần IDENTITY_INSERT vì StoreID có thể không phải IDENTITY)
ALTER TABLE Store ALTER COLUMN ManagerID INT NULL;
ALTER TABLE Store DROP CONSTRAINT UQ__Store__3BA2AA80A74CDFE4;

SET IDENTITY_INSERT Store ON;
INSERT INTO Store (StoreID, StoreName, StoreDescription, Location, ManagerID, CreatedDate, ImagePath, StoreEmail, StorePhone, OperatingHours)
VALUES 
    (1, 'Cửa hàng Quận 1', 'Chi nhánh trung tâm TP.HCM', 'Quận 1, TP. Hồ Chí Minh', NULL, '2024-01-01', 'store1.jpg', 'q1@shop.com', '0901234567', '08:00-22:00'),
    (2, 'Cửa hàng Quận 9', 'Chi nhánh phía Đông TP.HCM', 'Quận 9, TP. Hồ Chí Minh', NULL, '2024-01-05', 'store2.jpg', 'q9@shop.com', '0907654321', '09:00-21:00');
SET IDENTITY_INSERT Store OFF;

-- Chèn dữ liệu vào bảng StoreStock
INSERT INTO StoreStock (StoreID, VariantID, StockQuantity)
VALUES 
    (1, 1, 10), (1, 2, 8), (1, 3, 5),
    (1, 4, 12), (1, 5, 10), (1, 6, 7),
    (2, 1, 15), (2, 2, 13), (2, 3, 9),
    (2, 4, 14), (2, 5, 11), (2, 6, 8);
ALTER TABLE [Order]
ADD 
    Country NVARCHAR(100) NULL,
    Province NVARCHAR(100) NULL;

ALTER TABLE [ShippingAddress]
ADD 
    RecipientName NVARCHAR(255) NOT NULL DEFAULT N'',
    RecipientPhone NVARCHAR(20) NOT NULL DEFAULT N'',
    Province NVARCHAR(100) NULL,
    District NVARCHAR(100) NULL,
    Email NVARCHAR(255) NULL;
CREATE TABLE ProductImage (
    ProductImageId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    ImagePath NVARCHAR(255) NOT NULL,
    IsMain BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_ProductImage_Product FOREIGN KEY (ProductId)
        REFERENCES Product(ProductId)
);

GO
-- Giả sử bảng Product đã có 7 sản phẩm, ta thêm 3 hình ảnh cho mỗi sản phẩm
INSERT INTO ProductImage (ProductId, ImagePath, IsMain, CreatedDate) VALUES
(1, 'images/product_1_main.jpg', 1, GETDATE()),
(2, 'images/product_2_main.jpg', 1, GETDATE()),
(3, 'images/product_3_main.jpg', 1, GETDATE()),
(4, 'images/product_4_main.jpg', 1, GETDATE()),
(5, 'images/product_5_main.jpg', 1, GETDATE()),
(6, 'images/product_6_main.jpg', 1, GETDATE()),
(7, 'images/product_7_main.jpg', 1, GETDATE());
GO

-- Thêm ảnh phụ cho từng sản phẩm
INSERT INTO ProductImage (ProductId, ImagePath, IsMain, CreatedDate) VALUES
(1, 'images/product_1_1.jpg', 0, GETDATE()),
(1, 'images/product_1_2.jpg', 0, GETDATE()),

(2, 'images/product_2_1.jpg', 0, GETDATE()),
(2, 'images/product_2_2.jpg', 0, GETDATE()),

(3, 'images/product_3_1.jpg', 0, GETDATE()),
(3, 'images/product_3_2.jpg', 0, GETDATE()),

(4, 'images/product_4_1.jpg', 0, GETDATE()),
(4, 'images/product_4_2.jpg', 0, GETDATE()),

(5, 'images/product_5_1.jpg', 0, GETDATE()),
(5, 'images/product_5_2.jpg', 0, GETDATE()),

(6, 'images/product_6_1.jpg', 0, GETDATE()),
(6, 'images/product_6_2.jpg', 0, GETDATE()),

(7, 'images/product_7_1.jpg', 0, GETDATE()),
(7, 'images/product_7_2.jpg', 0, GETDATE());
GO



