using System;
using System.Collections.Generic;

namespace OneTouch.Models;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public int? ScheduleId { get; set; }

    public int? UserId { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual Invoice? Invoice { get; set; }

    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual Schedule? Schedule { get; set; }

    public virtual User? User { get; set; }
}
