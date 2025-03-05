public class CreateInventoryImportDto
{
    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }
    public string? Status { get; set; }
    public string? ReferenceNumber { get; set; }
    public decimal? TotalCost { get; set; }

    // Danh sách lịch sử nhập hàng
    public List<CreateInventoryImportHistoryDto> Histories { get; set; } = new List<CreateInventoryImportHistoryDto>();

    // Danh sách chi tiết nhập hàng
    public List<CreateInventoryImportDetailDto> Details { get; set; } = new List<CreateInventoryImportDetailDto>();
}

// DTO chi tiết nhập hàng
public class CreateInventoryImportDetailDto
{
    public int ProductVariantId { get; set; }
    public int Quantity { get; set; }

    // Danh sách phân bổ số lượng cho từng cửa hàng cho sản phẩm này
    public List<CreateInventoryImportStoreDetailDto> StoreAllocations { get; set; } = new List<CreateInventoryImportStoreDetailDto>();
}

// DTO phân bổ số lượng cho từng cửa hàng
public class CreateInventoryImportStoreDetailDto
{
    public int StoreId { get; set; }
    public int AllocatedQuantity { get; set; }
    public string? Status { get; set; }
    public string? Comments { get; set; }
}

public class CreateInventoryImportHistoryDto
{
    public string Status { get; set; } = null!;
    public int ChangedBy { get; set; }
    public DateTime? ChangedDate { get; set; }
    public string? Comments { get; set; }
}
