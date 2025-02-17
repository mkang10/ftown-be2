using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ReturnRequest
{
    public int ReturnRequestId { get; set; }

    public int OrderId { get; set; }

    public string Reason { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Order Order { get; set; } = null!;
}
