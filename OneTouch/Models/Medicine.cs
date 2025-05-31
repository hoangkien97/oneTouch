using System;
using System.Collections.Generic;

namespace OneTouch.Models;

public partial class Medicine
{
    public int MedicineId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Unit { get; set; }

    public decimal? Price { get; set; }

    public virtual ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();
}
