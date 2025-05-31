using System;
using System.Collections.Generic;

namespace OneTouch.Models;

public partial class MedicalRecord
{
    public int RecordId { get; set; }

    public int? AppointmentId { get; set; }

    public string? Diagnosis { get; set; }

    public string? DoctorNote { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Appointment? Appointment { get; set; }
}
