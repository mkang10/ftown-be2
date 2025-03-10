using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ReturnOrderMedium
{
    public int ReturnOrderMediaId { get; set; }

    public int ReturnOrderId { get; set; }

    public string MediaUrl { get; set; } = null!;

    public string MediaType { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public virtual ReturnOrder ReturnOrder { get; set; } = null!;
}
