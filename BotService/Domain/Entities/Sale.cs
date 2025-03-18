using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Sale
{
    public int SaleId { get; set; }

    public string SaleName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal? DiscountRate { get; set; }

    public bool? IsActive { get; set; }
}
