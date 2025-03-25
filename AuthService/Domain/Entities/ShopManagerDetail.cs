using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class ShopManagerDetail
{
    public int ShopManagerDetailId { get; set; }

    public int AccountId { get; set; }

    public int StoreId { get; set; }

    public DateTime? ManagedDate { get; set; }

    public int? YearsOfExperience { get; set; }

    public string? ManagerCertifications { get; set; }

    public string? OfficeContact { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<CheckDetail> CheckDetails { get; set; } = new List<CheckDetail>();

    public virtual ICollection<Import> Imports { get; set; } = new List<Import>();
}
