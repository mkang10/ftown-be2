USE [master]
GO
/****** Object:  Database [Ftown]    Script Date: 3/13/2025 11:04:34 AM ******/
CREATE DATABASE [Ftown]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Ftown', FILENAME = N'E:\ssms\DataSSMS\Ftown.mdf' , SIZE = 73728KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
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
/****** Object:  Table [dbo].[Account]    Script Date: 3/13/2025 11:04:34 AM ******/
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
/****** Object:  Table [dbo].[AccountInterests]    Script Date: 3/13/2025 11:04:34 AM ******/
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
/****** Object:  Table [dbo].[AuditLog]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AuditLog](
	[AuditLogID] [int] IDENTITY(1,1) NOT NULL,
	[TableName] [nvarchar](100) NOT NULL,
	[RecordID] [nvarchar](100) NOT NULL,
	[Operation] [nvarchar](50) NOT NULL,
	[ChangeDate] [datetime] NOT NULL,
	[ChangedBy] [int] NULL,
	[ChangeData] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[AuditLogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CartItems]    Script Date: 3/13/2025 11:04:34 AM ******/
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
/****** Object:  Table [dbo].[Category]    Script Date: 3/13/2025 11:04:34 AM ******/
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
/****** Object:  Table [dbo].[Color]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Color](
	[ColorID] [int] IDENTITY(1,1) NOT NULL,
	[ColorName] [nvarchar](50) NOT NULL,
	[ColorCode] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ColorID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Conversation]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Conversation](
	[ConversationID] [int] IDENTITY(1,1) NOT NULL,
	[ConversationName] [nvarchar](255) NULL,
	[IsGroup] [bit] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ConversationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ConversationParticipants]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ConversationParticipants](
	[ConversationID] [int] NOT NULL,
	[AccountID] [int] NOT NULL,
	[JoinedDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ConversationID] ASC,
	[AccountID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CustomerDetail]    Script Date: 3/13/2025 11:04:34 AM ******/
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
/****** Object:  Table [dbo].[DeliveryTracking]    Script Date: 3/13/2025 11:04:34 AM ******/
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
/****** Object:  Table [dbo].[Feedback]    Script Date: 3/13/2025 11:04:34 AM ******/
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
/****** Object:  Table [dbo].[Interests]    Script Date: 3/13/2025 11:04:34 AM ******/
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
/****** Object:  Table [dbo].[Message]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Message](
	[MessageID] [int] IDENTITY(1,1) NOT NULL,
	[ConversationID] [int] NOT NULL,
	[SenderID] [int] NOT NULL,
	[MessageContent] [nvarchar](max) NOT NULL,
	[SentDate] [datetime] NOT NULL,
	[ParentMessageID] [int] NULL,
	[IsRead] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MessageID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Notification]    Script Date: 3/13/2025 11:04:34 AM ******/
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
/****** Object:  Table [dbo].[Order]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Order](
	[OrderID] [int] IDENTITY(1,1) NOT NULL,
	[AccountID] [int] NOT NULL,
	[WareHouseID] [int] NULL,
	[ShippingAddressID] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[Status] [nvarchar](50) NULL,
	[OrderTotal] [decimal](10, 2) NULL,
	[ShippingCost] [decimal](10, 2) NULL,
	[Tax] [decimal](10, 2) NULL,
	[DeliveryMethod] [nvarchar](100) NULL,
	[FullName] [nvarchar](255) NOT NULL,
	[Email] [nvarchar](255) NOT NULL,
	[PhoneNumber] [nvarchar](15) NOT NULL,
	[Address] [nvarchar](500) NOT NULL,
	[City] [nvarchar](100) NOT NULL,
	[District] [nvarchar](100) NOT NULL,
	[Country] [nvarchar](100) NULL,
	[Province] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[OrderID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderDetails]    Script Date: 3/13/2025 11:04:34 AM ******/
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
/****** Object:  Table [dbo].[Payment]    Script Date: 3/13/2025 11:04:34 AM ******/
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
/****** Object:  Table [dbo].[Product]    Script Date: 3/13/2025 11:04:34 AM ******/
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
	[SizeID] [int] NULL,
	[ColorID] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[ProductID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductImage]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductImage](
	[ProductImageId] [int] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NOT NULL,
	[ImagePath] [nvarchar](255) NOT NULL,
	[IsMain] [bit] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ProductImageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductVariant]    Script Date: 3/13/2025 11:04:34 AM ******/
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
	[ImagePath] [nvarchar](max) NULL,
	[SKU] [nvarchar](100) NULL,
	[Barcode] [nvarchar](100) NULL,
	[Weight] [decimal](10, 2) NULL,
	[WarehouseStockID] [int] NULL,
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
/****** Object:  Table [dbo].[ReplyFeedback]    Script Date: 3/13/2025 11:04:34 AM ******/
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
/****** Object:  Table [dbo].[ReturnOrder]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReturnOrder](
	[ReturnOrderId] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NOT NULL,
	[AccountId] [int] NOT NULL,
	[Email] [nvarchar](256) NOT NULL,
	[TotalRefundAmount] [decimal](18, 2) NOT NULL,
	[ReturnReason] [nvarchar](max) NOT NULL,
	[ReturnOption] [nvarchar](50) NOT NULL,
	[ReturnDescription] [nvarchar](max) NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[BankName] [nvarchar](255) NULL,
	[BankAccountNumber] [nvarchar](50) NULL,
	[BankAccountName] [nvarchar](255) NULL,
	[RefundMethod] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ReturnOrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReturnOrderHistory]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReturnOrderHistory](
	[ReturnOrderHistoryID] [int] IDENTITY(1,1) NOT NULL,
	[ReturnOrderID] [int] NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[ChangedBy] [nvarchar](255) NOT NULL,
	[ChangedDate] [datetime2](7) NOT NULL,
	[Comments] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[ReturnOrderHistoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReturnOrderItem]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReturnOrderItem](
	[ReturnOrderItemId] [int] IDENTITY(1,1) NOT NULL,
	[ReturnOrderId] [int] NOT NULL,
	[ProductVariantId] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[RefundPrice] [decimal](18, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ReturnOrderItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReturnOrderMedia]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReturnOrderMedia](
	[ReturnOrderMediaId] [int] IDENTITY(1,1) NOT NULL,
	[ReturnOrderId] [int] NOT NULL,
	[MediaUrl] [nvarchar](2048) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ReturnOrderMediaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Role]    Script Date: 3/13/2025 11:04:34 AM ******/
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
/****** Object:  Table [dbo].[Sale]    Script Date: 3/13/2025 11:04:34 AM ******/
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
/****** Object:  Table [dbo].[ShippingAddress]    Script Date: 3/13/2025 11:04:34 AM ******/
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
	[RecipientName] [nvarchar](255) NOT NULL,
	[RecipientPhone] [nvarchar](20) NOT NULL,
	[Province] [nvarchar](100) NULL,
	[District] [nvarchar](100) NULL,
	[Email] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[AddressID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShopManagerDetail]    Script Date: 3/13/2025 11:04:34 AM ******/
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
/****** Object:  Table [dbo].[ShoppingCart]    Script Date: 3/13/2025 11:04:34 AM ******/
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
/****** Object:  Table [dbo].[Size]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Size](
	[SizeID] [int] IDENTITY(1,1) NOT NULL,
	[SizeName] [nvarchar](50) NOT NULL,
	[SizeDescription] [nvarchar](255) NULL,
	[CreatedDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[SizeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StaffDetail]    Script Date: 3/13/2025 11:04:34 AM ******/
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
/****** Object:  Table [dbo].[StoreCheckDetail]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StoreCheckDetail](
	[CheckDetailID] [int] IDENTITY(1,1) NOT NULL,
	[CheckSessionID] [int] NOT NULL,
	[StoreID] [int] NOT NULL,
	[VariantOfflineId] [int] NOT NULL,
	[StaffID] [int] NOT NULL,
	[ExpectedQuantity] [int] NOT NULL,
	[CountedQuantity] [int] NOT NULL,
	[Difference]  AS ([CountedQuantity]-[ExpectedQuantity]),
	[Comments] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[CheckDetailID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StoreCheckSession]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StoreCheckSession](
	[CheckSessionID] [int] IDENTITY(1,1) NOT NULL,
	[OwnerID] [int] NOT NULL,
	[SessionDate] [datetime] NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[Remarks] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[CheckSessionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StoreImport]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StoreImport](
	[ImportID] [int] IDENTITY(1,1) NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedDate] [datetime] NULL,
	[Status] [nvarchar](50) NULL,
	[ReferenceNumber] [nvarchar](100) NULL,
	[TotalCost] [decimal](10, 2) NULL,
	[ApprovedDate] [datetime] NULL,
	[CompletedDate] [datetime] NULL,
	[OriginalImportID] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[ImportID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StoreImportDetails]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StoreImportDetails](
	[ImportDetailID] [int] IDENTITY(1,1) NOT NULL,
	[ImportID] [int] NOT NULL,
	[ProductVariantOfflineID] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ImportDetailID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StoreImportStoreDetail]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StoreImportStoreDetail](
	[ImportDetailID] [int] NOT NULL,
	[StoreID] [int] NOT NULL,
	[AllocatedQuantity] [int] NOT NULL,
	[Status] [nchar](10) NULL,
	[Comments] [nvarchar](500) NULL,
	[StaffDetailID] [int] NULL,
 CONSTRAINT [PK_InventoryImportStoreDetail] PRIMARY KEY CLUSTERED 
(
	[ImportDetailID] ASC,
	[StoreID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TransferOrder]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TransferOrder](
	[TransferOrderID] [int] IDENTITY(1,1) NOT NULL,
	[SourceStoreID] [int] NOT NULL,
	[DestinationStoreID] [int] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedDate] [datetime] NULL,
	[Status] [nvarchar](50) NOT NULL,
	[Remarks] [nvarchar](500) NULL,
	[OriginalTransferOrderID] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[TransferOrderID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TransferOrderDetails]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TransferOrderDetails](
	[TransferOrderDetailID] [int] IDENTITY(1,1) NOT NULL,
	[TransferOrderID] [int] NOT NULL,
	[SourceStoreID] [int] NOT NULL,
	[ProductVariantOfflineID] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[DeliveredQuantity] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[TransferOrderDetailID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Warehouses]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Warehouses](
	[WarehouseID] [int] IDENTITY(1,1) NOT NULL,
	[WarehouseName] [nvarchar](255) NOT NULL,
	[WarehouseDescription] [nvarchar](500) NULL,
	[Location] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NULL,
	[ImagePath] [nvarchar](max) NULL,
	[Email] [nvarchar](255) NULL,
	[Phone] [nvarchar](50) NULL,
	[WarehouseType] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[WarehouseID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WareHousesStock]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WareHousesStock](
	[WareHouseStockID] [int] NOT NULL,
	[VariantID] [int] NOT NULL,
	[StockQuantity] [int] NOT NULL,
	[WareHouseID] [int] NOT NULL,
 CONSTRAINT [PK_WareHousesStock] PRIMARY KEY CLUSTERED 
(
	[WareHouseStockID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WareHouseStockAudit]    Script Date: 3/13/2025 11:04:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WareHouseStockAudit](
	[AuditID] [int] IDENTITY(1,1) NOT NULL,
	[WareHouseStockID] [int] NOT NULL,
	[Action] [nvarchar](100) NOT NULL,
	[QuantityChange] [int] NOT NULL,
	[ActionDate] [datetime] NOT NULL,
	[ChangedBy] [int] NULL,
	[Note] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[AuditID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WishList]    Script Date: 3/13/2025 11:04:34 AM ******/
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
/****** Object:  Table [dbo].[WishListItems]    Script Date: 3/13/2025 11:04:34 AM ******/
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
/****** Object:  Index [IX_Account_Email]    Script Date: 3/13/2025 11:04:34 AM ******/
CREATE NONCLUSTERED INDEX [IX_Account_Email] ON [dbo].[Account]
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Category_ParentCategoryID]    Script Date: 3/13/2025 11:04:34 AM ******/
CREATE NONCLUSTERED INDEX [IX_Category_ParentCategoryID] ON [dbo].[Category]
(
	[ParentCategoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Order_AccountID]    Script Date: 3/13/2025 11:04:34 AM ******/
CREATE NONCLUSTERED INDEX [IX_Order_AccountID] ON [dbo].[Order]
(
	[AccountID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Payment_OrderID]    Script Date: 3/13/2025 11:04:34 AM ******/
CREATE NONCLUSTERED INDEX [IX_Payment_OrderID] ON [dbo].[Payment]
(
	[OrderID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Product_CategoryID]    Script Date: 3/13/2025 11:04:34 AM ******/
CREATE NONCLUSTERED INDEX [IX_Product_CategoryID] ON [dbo].[Product]
(
	[CategoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductVariant_ProductID]    Script Date: 3/13/2025 11:04:34 AM ******/
CREATE NONCLUSTERED INDEX [IX_ProductVariant_ProductID] ON [dbo].[ProductVariant]
(
	[ProductID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ShippingAddress_AccountID]    Script Date: 3/13/2025 11:04:34 AM ******/
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
ALTER TABLE [dbo].[AuditLog] ADD  DEFAULT (getdate()) FOR [ChangeDate]
GO
ALTER TABLE [dbo].[Category] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Category] ADD  DEFAULT ((0)) FOR [DisplayOrder]
GO
ALTER TABLE [dbo].[Color] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Conversation] ADD  DEFAULT ((0)) FOR [IsGroup]
GO
ALTER TABLE [dbo].[Conversation] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Conversation] ADD  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[ConversationParticipants] ADD  DEFAULT (getdate()) FOR [JoinedDate]
GO
ALTER TABLE [dbo].[CustomerDetail] ADD  DEFAULT ((0)) FOR [LoyaltyPoints]
GO
ALTER TABLE [dbo].[CustomerDetail] ADD  DEFAULT ('Basic') FOR [MembershipLevel]
GO
ALTER TABLE [dbo].[DeliveryTracking] ADD  DEFAULT ('In Transit') FOR [Status]
GO
ALTER TABLE [dbo].[DeliveryTracking] ADD  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Feedback] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Message] ADD  DEFAULT (getdate()) FOR [SentDate]
GO
ALTER TABLE [dbo].[Message] ADD  DEFAULT ((0)) FOR [IsRead]
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
ALTER TABLE [dbo].[Payment] ADD  DEFAULT ('Pending') FOR [PaymentStatus]
GO
ALTER TABLE [dbo].[Payment] ADD  DEFAULT (getdate()) FOR [TransactionDate]
GO
ALTER TABLE [dbo].[ProductImage] ADD  DEFAULT ((0)) FOR [IsMain]
GO
ALTER TABLE [dbo].[ProductImage] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[ReplyFeedback] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[ReturnOrder] ADD  DEFAULT ('Pending') FOR [Status]
GO
ALTER TABLE [dbo].[ReturnOrder] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[ReturnOrder] ADD  DEFAULT ('Bank Transfer') FOR [RefundMethod]
GO
ALTER TABLE [dbo].[ReturnOrderHistory] ADD  DEFAULT (sysutcdatetime()) FOR [ChangedDate]
GO
ALTER TABLE [dbo].[ReturnOrderMedia] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Sale] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[ShippingAddress] ADD  DEFAULT ((0)) FOR [IsDefault]
GO
ALTER TABLE [dbo].[ShippingAddress] ADD  DEFAULT (N'') FOR [RecipientName]
GO
ALTER TABLE [dbo].[ShippingAddress] ADD  DEFAULT (N'') FOR [RecipientPhone]
GO
ALTER TABLE [dbo].[ShopManagerDetail] ADD  DEFAULT (getdate()) FOR [ManagedDate]
GO
ALTER TABLE [dbo].[ShoppingCart] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Size] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[StaffDetail] ADD  DEFAULT (getdate()) FOR [JoinDate]
GO
ALTER TABLE [dbo].[StoreImport] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[StoreImport] ADD  DEFAULT ('Pending') FOR [Status]
GO
ALTER TABLE [dbo].[WareHouseStockAudit] ADD  DEFAULT (getdate()) FOR [ActionDate]
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
ALTER TABLE [dbo].[AuditLog]  WITH CHECK ADD  CONSTRAINT [FK_AuditLog_Account] FOREIGN KEY([ChangedBy])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[AuditLog] CHECK CONSTRAINT [FK_AuditLog_Account]
GO
ALTER TABLE [dbo].[CartItems]  WITH CHECK ADD FOREIGN KEY([CartID])
REFERENCES [dbo].[ShoppingCart] ([CartID])
GO
ALTER TABLE [dbo].[CartItems]  WITH CHECK ADD FOREIGN KEY([ProductVariantID])
REFERENCES [dbo].[ProductVariant] ([VariantID])
GO
ALTER TABLE [dbo].[ConversationParticipants]  WITH CHECK ADD  CONSTRAINT [FK_ConversationParticipants_Account] FOREIGN KEY([AccountID])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[ConversationParticipants] CHECK CONSTRAINT [FK_ConversationParticipants_Account]
GO
ALTER TABLE [dbo].[ConversationParticipants]  WITH CHECK ADD  CONSTRAINT [FK_ConversationParticipants_Conversation] FOREIGN KEY([ConversationID])
REFERENCES [dbo].[Conversation] ([ConversationID])
GO
ALTER TABLE [dbo].[ConversationParticipants] CHECK CONSTRAINT [FK_ConversationParticipants_Conversation]
GO
ALTER TABLE [dbo].[CustomerDetail]  WITH CHECK ADD FOREIGN KEY([AccountID])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[DeliveryTracking]  WITH CHECK ADD FOREIGN KEY([OrderID])
REFERENCES [dbo].[Order] ([OrderID])
GO
ALTER TABLE [dbo].[Feedback]  WITH CHECK ADD FOREIGN KEY([AccountID])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[Feedback]  WITH CHECK ADD FOREIGN KEY([ProductID])
REFERENCES [dbo].[Product] ([ProductID])
GO
ALTER TABLE [dbo].[Message]  WITH CHECK ADD  CONSTRAINT [FK_Message_Conversation] FOREIGN KEY([ConversationID])
REFERENCES [dbo].[Conversation] ([ConversationID])
GO
ALTER TABLE [dbo].[Message] CHECK CONSTRAINT [FK_Message_Conversation]
GO
ALTER TABLE [dbo].[Message]  WITH CHECK ADD  CONSTRAINT [FK_Message_Parent] FOREIGN KEY([ParentMessageID])
REFERENCES [dbo].[Message] ([MessageID])
GO
ALTER TABLE [dbo].[Message] CHECK CONSTRAINT [FK_Message_Parent]
GO
ALTER TABLE [dbo].[Message]  WITH CHECK ADD  CONSTRAINT [FK_Message_Sender] FOREIGN KEY([SenderID])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[Message] CHECK CONSTRAINT [FK_Message_Sender]
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
ALTER TABLE [dbo].[Order]  WITH CHECK ADD  CONSTRAINT [FK_Order_Warehouses] FOREIGN KEY([WareHouseID])
REFERENCES [dbo].[Warehouses] ([WarehouseID])
GO
ALTER TABLE [dbo].[Order] CHECK CONSTRAINT [FK_Order_Warehouses]
GO
ALTER TABLE [dbo].[OrderDetails]  WITH CHECK ADD FOREIGN KEY([OrderID])
REFERENCES [dbo].[Order] ([OrderID])
GO
ALTER TABLE [dbo].[OrderDetails]  WITH CHECK ADD FOREIGN KEY([ProductVariantID])
REFERENCES [dbo].[ProductVariant] ([VariantID])
GO
ALTER TABLE [dbo].[Payment]  WITH CHECK ADD FOREIGN KEY([OrderID])
REFERENCES [dbo].[Order] ([OrderID])
GO
ALTER TABLE [dbo].[Product]  WITH CHECK ADD FOREIGN KEY([CategoryID])
REFERENCES [dbo].[Category] ([CategoryID])
GO
ALTER TABLE [dbo].[Product]  WITH CHECK ADD  CONSTRAINT [FK_Product_Color] FOREIGN KEY([ColorID])
REFERENCES [dbo].[Color] ([ColorID])
GO
ALTER TABLE [dbo].[Product] CHECK CONSTRAINT [FK_Product_Color]
GO
ALTER TABLE [dbo].[Product]  WITH CHECK ADD  CONSTRAINT [FK_Product_Size] FOREIGN KEY([SizeID])
REFERENCES [dbo].[Size] ([SizeID])
GO
ALTER TABLE [dbo].[Product] CHECK CONSTRAINT [FK_Product_Size]
GO
ALTER TABLE [dbo].[ProductImage]  WITH CHECK ADD  CONSTRAINT [FK_ProductImage_Product] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Product] ([ProductID])
GO
ALTER TABLE [dbo].[ProductImage] CHECK CONSTRAINT [FK_ProductImage_Product]
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
ALTER TABLE [dbo].[ReturnOrder]  WITH CHECK ADD  CONSTRAINT [FK_ReturnOrder_Order] FOREIGN KEY([OrderId])
REFERENCES [dbo].[Order] ([OrderID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ReturnOrder] CHECK CONSTRAINT [FK_ReturnOrder_Order]
GO
ALTER TABLE [dbo].[ReturnOrderHistory]  WITH CHECK ADD  CONSTRAINT [FK_ReturnOrderHistory_ReturnOrder] FOREIGN KEY([ReturnOrderID])
REFERENCES [dbo].[ReturnOrder] ([ReturnOrderId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ReturnOrderHistory] CHECK CONSTRAINT [FK_ReturnOrderHistory_ReturnOrder]
GO
ALTER TABLE [dbo].[ReturnOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_ReturnOrderItem_ProductVariant] FOREIGN KEY([ProductVariantId])
REFERENCES [dbo].[ProductVariant] ([VariantID])
GO
ALTER TABLE [dbo].[ReturnOrderItem] CHECK CONSTRAINT [FK_ReturnOrderItem_ProductVariant]
GO
ALTER TABLE [dbo].[ReturnOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_ReturnOrderItem_ReturnOrder] FOREIGN KEY([ReturnOrderId])
REFERENCES [dbo].[ReturnOrder] ([ReturnOrderId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ReturnOrderItem] CHECK CONSTRAINT [FK_ReturnOrderItem_ReturnOrder]
GO
ALTER TABLE [dbo].[ReturnOrderMedia]  WITH CHECK ADD  CONSTRAINT [FK_ReturnOrderMedia_ReturnOrder] FOREIGN KEY([ReturnOrderId])
REFERENCES [dbo].[ReturnOrder] ([ReturnOrderId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ReturnOrderMedia] CHECK CONSTRAINT [FK_ReturnOrderMedia_ReturnOrder]
GO
ALTER TABLE [dbo].[ShippingAddress]  WITH CHECK ADD FOREIGN KEY([AccountID])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[ShopManagerDetail]  WITH CHECK ADD FOREIGN KEY([AccountID])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[ShoppingCart]  WITH CHECK ADD FOREIGN KEY([AccountID])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[StaffDetail]  WITH CHECK ADD FOREIGN KEY([AccountID])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[StoreCheckDetail]  WITH CHECK ADD  CONSTRAINT [FK_InventoryCheckDetail_Session] FOREIGN KEY([CheckSessionID])
REFERENCES [dbo].[StoreCheckSession] ([CheckSessionID])
GO
ALTER TABLE [dbo].[StoreCheckDetail] CHECK CONSTRAINT [FK_InventoryCheckDetail_Session]
GO
ALTER TABLE [dbo].[StoreCheckDetail]  WITH CHECK ADD  CONSTRAINT [FK_InventoryCheckDetail_Staff] FOREIGN KEY([StaffID])
REFERENCES [dbo].[StaffDetail] ([StaffDetailID])
GO
ALTER TABLE [dbo].[StoreCheckDetail] CHECK CONSTRAINT [FK_InventoryCheckDetail_Staff]
GO
ALTER TABLE [dbo].[StoreCheckSession]  WITH CHECK ADD  CONSTRAINT [FK_InventoryCheckSession_Owner] FOREIGN KEY([OwnerID])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[StoreCheckSession] CHECK CONSTRAINT [FK_InventoryCheckSession_Owner]
GO
ALTER TABLE [dbo].[StoreImport]  WITH CHECK ADD FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[StoreImport]  WITH CHECK ADD  CONSTRAINT [FK_StoreImport_OriginalImport] FOREIGN KEY([OriginalImportID])
REFERENCES [dbo].[StoreImport] ([ImportID])
GO
ALTER TABLE [dbo].[StoreImport] CHECK CONSTRAINT [FK_StoreImport_OriginalImport]
GO
ALTER TABLE [dbo].[StoreImportDetails]  WITH CHECK ADD FOREIGN KEY([ImportID])
REFERENCES [dbo].[StoreImport] ([ImportID])
GO
ALTER TABLE [dbo].[StoreImportStoreDetail]  WITH CHECK ADD  CONSTRAINT [FK_InventoryImportStoreDetail_ImportDetail] FOREIGN KEY([ImportDetailID])
REFERENCES [dbo].[StoreImportDetails] ([ImportDetailID])
GO
ALTER TABLE [dbo].[StoreImportStoreDetail] CHECK CONSTRAINT [FK_InventoryImportStoreDetail_ImportDetail]
GO
ALTER TABLE [dbo].[StoreImportStoreDetail]  WITH CHECK ADD  CONSTRAINT [FK_InventoryImportStoreDetail_StaffDetail] FOREIGN KEY([StaffDetailID])
REFERENCES [dbo].[StaffDetail] ([StaffDetailID])
GO
ALTER TABLE [dbo].[StoreImportStoreDetail] CHECK CONSTRAINT [FK_InventoryImportStoreDetail_StaffDetail]
GO
ALTER TABLE [dbo].[TransferOrder]  WITH CHECK ADD  CONSTRAINT [FK_TransferOrder_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Account] ([AccountID])
GO
ALTER TABLE [dbo].[TransferOrder] CHECK CONSTRAINT [FK_TransferOrder_CreatedBy]
GO
ALTER TABLE [dbo].[TransferOrder]  WITH CHECK ADD  CONSTRAINT [FK_TransferOrder_OriginalTransferOrder] FOREIGN KEY([OriginalTransferOrderID])
REFERENCES [dbo].[TransferOrder] ([TransferOrderID])
GO
ALTER TABLE [dbo].[TransferOrder] CHECK CONSTRAINT [FK_TransferOrder_OriginalTransferOrder]
GO
ALTER TABLE [dbo].[TransferOrderDetails]  WITH CHECK ADD  CONSTRAINT [FK_TransferOrderDetails_TransferOrder] FOREIGN KEY([TransferOrderID])
REFERENCES [dbo].[TransferOrder] ([TransferOrderID])
GO
ALTER TABLE [dbo].[TransferOrderDetails] CHECK CONSTRAINT [FK_TransferOrderDetails_TransferOrder]
GO
ALTER TABLE [dbo].[WareHousesStock]  WITH CHECK ADD  CONSTRAINT [FK_WareHousesStock_ProductVariant] FOREIGN KEY([VariantID])
REFERENCES [dbo].[ProductVariant] ([VariantID])
GO
ALTER TABLE [dbo].[WareHousesStock] CHECK CONSTRAINT [FK_WareHousesStock_ProductVariant]
GO
ALTER TABLE [dbo].[WareHousesStock]  WITH CHECK ADD  CONSTRAINT [FK_WareHousesStock_Warehouses] FOREIGN KEY([WareHouseID])
REFERENCES [dbo].[Warehouses] ([WarehouseID])
GO
ALTER TABLE [dbo].[WareHousesStock] CHECK CONSTRAINT [FK_WareHousesStock_Warehouses]
GO
ALTER TABLE [dbo].[WareHouseStockAudit]  WITH CHECK ADD  CONSTRAINT [FK_WareHouseStockAudit_WareHousesStock] FOREIGN KEY([WareHouseStockID])
REFERENCES [dbo].[WareHousesStock] ([WareHouseStockID])
GO
ALTER TABLE [dbo].[WareHouseStockAudit] CHECK CONSTRAINT [FK_WareHouseStockAudit_WareHousesStock]
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
ALTER TABLE [dbo].[ReturnOrderItem]  WITH CHECK ADD CHECK  (([Quantity]>(0)))
GO
ALTER TABLE [dbo].[ReturnOrderItem]  WITH CHECK ADD CHECK  (([RefundPrice]>=(0)))
GO
ALTER TABLE [dbo].[Sale]  WITH CHECK ADD CHECK  (([DiscountRate]>=(0) AND [DiscountRate]<=(100)))
GO
USE [master]
GO
ALTER DATABASE [Ftown] SET  READ_WRITE 
GO
