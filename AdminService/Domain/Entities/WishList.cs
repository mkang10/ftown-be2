using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class WishList
{
    public int WishListId { get; set; }

    public int AccountId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<WishListItem> WishListItems { get; set; } = new List<WishListItem>();
}
