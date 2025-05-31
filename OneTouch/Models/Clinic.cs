using System;
using System.Collections.Generic;

namespace OneTouch.Models;

public partial class Clinic
{
    public int ClinicId { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? City { get; set; }

    public string? District { get; set; }

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
