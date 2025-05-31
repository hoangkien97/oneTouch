using System;
using System.Collections.Generic;

namespace OneTouch.Models;

public partial class UserInsurance
{
    public int InsuranceId { get; set; }

    public int? UserId { get; set; }

    public string InsuranceNumber { get; set; } = null!;

    public DateOnly? ExpirationDate { get; set; }

    public int? CoveragePercent { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual User? User { get; set; }
}
