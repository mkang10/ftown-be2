﻿using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class FavoriteProduct
{
    public int FavoriteId { get; set; }

    public int AccountId { get; set; }

    public int ProductId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
