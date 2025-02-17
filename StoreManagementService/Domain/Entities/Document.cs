using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Document
{
    public int DocumentId { get; set; }

    public int TransactionId { get; set; }

    public int UploadedBy { get; set; }

    public DateTime? UploadedDate { get; set; }

    public string? FilePath { get; set; }

    public string? ImagePath { get; set; }

    public virtual InventoryTransaction Transaction { get; set; } = null!;

    public virtual Account UploadedByNavigation { get; set; } = null!;
}
