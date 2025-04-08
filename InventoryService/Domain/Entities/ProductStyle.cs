using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ProductStyle
{
    public int ProductStyleId { get; set; }

    public int ProductId { get; set; }

    public int InterestId { get; set; }

    public virtual Interest Interest { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
