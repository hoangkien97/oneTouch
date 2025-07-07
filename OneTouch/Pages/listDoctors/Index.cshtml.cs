using Microsoft.AspNetCore.Mvc.RazorPages;
using OneTouch.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace OneTouch.Pages.listDoctors
{
    public class IndexModel : PageModel
    {
        private readonly OneTouchDbContext _context;
        public List<DoctorViewModel> Doctors { get; set; }

        public IndexModel(OneTouchDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            Doctors = _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Select(d => new DoctorViewModel
                {
                    DoctorId = d.DoctorId,
                    FullName = d.User.FullName,
                    AvatarPath = d.AvatarPath,
                    SpecialtyName = d.Specialty.Name,
                    Description = d.Description,
                    AverageRating = _context.Feedbacks
                        .Include(f => f.Appointment)
                            .ThenInclude(a => a.Schedule)
                        .Where(f => f.Appointment != null && f.Appointment.Schedule != null && f.Appointment.Schedule.DoctorId == d.DoctorId && f.Rating.HasValue)
                        .Any()
                            ? _context.Feedbacks
                                .Include(f => f.Appointment)
                                    .ThenInclude(a => a.Schedule)
                                .Where(f => f.Appointment != null && f.Appointment.Schedule != null && f.Appointment.Schedule.DoctorId == d.DoctorId && f.Rating.HasValue)
                                .Average(f => f.Rating.Value)
                            : 0,
                    TotalAppointments = _context.Appointments
                        .Include(a => a.Schedule)
                        .Count(a => a.Schedule != null && a.Schedule.DoctorId.HasValue && a.Schedule.DoctorId.Value == d.DoctorId)
                })
                .ToList();
        }

        public class DoctorViewModel
        {
            public int DoctorId { get; set; }
            public string FullName { get; set; }
            public string AvatarPath { get; set; }
            public string SpecialtyName { get; set; }
            public string Description { get; set; }
            public double AverageRating { get; set; }
            public int TotalAppointments { get; set; }
        }
    }
} 