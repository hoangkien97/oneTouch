using System;
using System.Collections.Generic;

namespace OneTouch.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }

    public string? Phone { get; set; }

    public string? Role { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Doctor? Doctor { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<UserInsurance> UserInsurances { get; set; } = new List<UserInsurance>();
}
