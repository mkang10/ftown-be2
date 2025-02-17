using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class WishListItem
{
    public int WishListItemId { get; set; }

    public int WishListId { get; set; }

    public int ProductVariantId { get; set; }

    public DateTime? AddedDate { get; set; }

    public virtual ProductVariant ProductVariant { get; set; } = null!;

    public virtual WishList WishList { get; set; } = null!;
}
