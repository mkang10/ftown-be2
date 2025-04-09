using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Interest
{
    public int InterestId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<AccountInterest> AccountInterests { get; set; } = new List<AccountInterest>();

    public virtual ICollection<ProductStyle> ProductStyles { get; set; } = new List<ProductStyle>();
}
