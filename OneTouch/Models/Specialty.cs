using System;
using System.Collections.Generic;

namespace OneTouch.Models;

public partial class Specialty
{
    public int SpecialtyId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
