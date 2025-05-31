using System;
using System.Collections.Generic;

namespace OneTouch.Models;

public partial class Doctor
{
    public int DoctorId { get; set; }

    public int? UserId { get; set; }

    public int? SpecialtyId { get; set; }

    public int? ClinicId { get; set; }

    public string? Description { get; set; }

    public int? ExperienceYears { get; set; }

    public string? Certificate { get; set; }

    public virtual Clinic? Clinic { get; set; }

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual Specialty? Specialty { get; set; }

    public virtual User? User { get; set; }
}
