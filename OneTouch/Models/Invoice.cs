using System;
using System.Collections.Generic;

namespace OneTouch.Models;

public partial class Invoice
{
    public int InvoiceId { get; set; }

    public int? AppointmentId { get; set; }

    public int? InsuranceId { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? InsuranceCoveredAmount { get; set; }

    public decimal? FinalAmount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? PaymentStatus { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual UserInsurance? Insurance { get; set; }
}
