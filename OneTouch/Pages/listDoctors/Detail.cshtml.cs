using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OneTouch.Models;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;

namespace OneTouch.Pages.listDoctors
{
    public class DetailModel : PageModel
    {
        private readonly OneTouchDbContext _context;
        public DoctorViewModel Doctor { get; set; }
        public List<FeedbackViewModel> Feedbacks { get; set; }

        public DetailModel(OneTouchDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet(int doctorId)
        {
            var doctor = _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .FirstOrDefault(d => d.DoctorId == doctorId);
            if (doctor == null)
                return NotFound();

            Doctor = new DoctorViewModel
            {
                DoctorId = doctor.DoctorId,
                FullName = doctor.User.FullName,
                AvatarPath = doctor.AvatarPath,
                SpecialtyName = doctor.Specialty.Name,
                Description = doctor.Description,
                AverageRating = _context.Feedbacks
                    .Include(f => f.Appointment)
                        .ThenInclude(a => a.Schedule)
                    .Where(f => f.Appointment != null && f.Appointment.Schedule != null && f.Appointment.Schedule.DoctorId == doctor.DoctorId && f.Rating.HasValue)
                    .Any()
                        ? _context.Feedbacks
                            .Include(f => f.Appointment)
                                .ThenInclude(a => a.Schedule)
                            .Where(f => f.Appointment != null && f.Appointment.Schedule != null && f.Appointment.Schedule.DoctorId == doctor.DoctorId && f.Rating.HasValue)
                            .Average(f => f.Rating.Value)
                        : 0,
                TotalAppointments = _context.Appointments
                    .Include(a => a.Schedule)
                    .Count(a => a.Schedule != null && a.Schedule.DoctorId == doctor.DoctorId)
            };

            Feedbacks = _context.Feedbacks
                .Include(f => f.Appointment)
                    .ThenInclude(a => a.Schedule)
                .Where(f => f.Appointment != null && f.Appointment.Schedule != null && f.Appointment.Schedule.DoctorId == doctorId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FeedbackViewModel
                {
                    PatientName = f.Appointment != null && f.Appointment.User != null ? f.Appointment.User.FullName : "(Ẩn danh)",
                    Rating = f.Rating.HasValue ? (int)f.Rating.Value : 0,
                    Comment = f.Comment,
                    CreatedAt = f.CreatedAt ?? DateTime.MinValue
                })
                .ToList();

            return Page();
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
        public class FeedbackViewModel
        {
            public string PatientName { get; set; }
            public int Rating { get; set; }
            public string Comment { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
} 