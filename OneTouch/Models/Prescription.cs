using System;
using System.Collections.Generic;

namespace OneTouch.Models;

public partial class Prescription
{
    public int PrescriptionId { get; set; }

    public int? AppointmentId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Note { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();
}
