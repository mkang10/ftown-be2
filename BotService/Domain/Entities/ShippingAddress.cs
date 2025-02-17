﻿using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ShippingAddress
{
    public int AddressId { get; set; }

    public int AccountId { get; set; }

    public string Address { get; set; } = null!;

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }

    public string? PostalCode { get; set; }

    public bool? IsDefault { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
