using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class StaffDetail
{
    public int StaffDetailId { get; set; }

    public int AccountId { get; set; }

    public int StoreId { get; set; }

    public DateTime? JoinDate { get; set; }

    public string Role { get; set; } = null!;   

    public string JobTitle { get; set; } = null!;

    public string Department { get; set; } = null!;

    public decimal? Salary { get; set; }

    public string? EmploymentType { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<CheckDetail> CheckDetails { get; set; } = new List<CheckDetail>();

    public virtual ICollection<ImportStoreDetail> ImportStoreDetails { get; set; } = new List<ImportStoreDetail>();

    public virtual ICollection<StoreExportStoreDetail> StoreExportStoreDetails { get; set; } = new List<StoreExportStoreDetail>();
}
