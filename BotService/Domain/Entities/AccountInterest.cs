﻿using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class AccountInterest
{
    public int AccountInterestId { get; set; }

    public int AccountId { get; set; }

    public int InterestId { get; set; }

    public int? InteractionCount { get; set; }

    public DateTime? LastUpdated { get; set; }

    public string Source { get; set; } = null!;

    public virtual Account Account { get; set; } = null!;

    public virtual Interest Interest { get; set; } = null!;
}
