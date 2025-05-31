using System;
using System.Collections.Generic;

namespace OneTouch.Models;

public partial class Schedule
{
    public int ScheduleId { get; set; }

    public int? DoctorId { get; set; }

    public DateOnly? Date { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public int? MaxPatients { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Doctor? Doctor { get; set; }
}
