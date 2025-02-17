﻿using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string? PaymentStatus { get; set; }

    public DateTime? TransactionDate { get; set; }

    public decimal Amount { get; set; }

    public string? PaymentReference { get; set; }

    public string? PaymentGatewayTransactionId { get; set; }

    public string? PaymentNotes { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ICollection<PaymentHistory> PaymentHistories { get; set; } = new List<PaymentHistory>();
}
