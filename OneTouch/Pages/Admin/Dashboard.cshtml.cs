using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OneTouch.Models;
using System.Linq;
using System.Collections.Generic;

namespace OneTouch.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly OneTouchDbContext _context;

        public DashboardModel(OneTouchDbContext context)
        {
            _context = context;
        }

        public string UserName { get; set; }
        public int TotalUsers { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalClinics { get; set; }
        public List<RecentAppointmentViewModel> RecentAppointments { get; set; }
        public List<RecentUserViewModel> RecentUsers { get; set; }

        public void OnGet()
        {
            // Lấy thông tin user từ session
            UserName = HttpContext.Session.GetString("UserName") ?? "Admin";

            // Tính toán thống kê
            TotalUsers = _context.Users.Count();
            TotalDoctors = _context.Doctors.Count();
            TotalAppointments = _context.Appointments.Count();
            TotalClinics = _context.Clinics.Count();

            // Lấy lịch hẹn gần đây (5 lịch hẹn mới nhất)
            RecentAppointments = _context.Appointments
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => new RecentAppointmentViewModel
                {
                    PatientName = a.User != null ? a.User.FullName : "",
                    DoctorName = a.Schedule != null && a.Schedule.Doctor != null && a.Schedule.Doctor.User != null ? a.Schedule.Doctor.User.FullName : "",
                    AppointmentDate = a.Schedule != null && a.Schedule.Date.HasValue && a.Schedule.StartTime.HasValue
                        ? (DateTime?)a.Schedule.Date.Value.ToDateTime(a.Schedule.StartTime.Value)
                        : null,
                    Status = a.Status
                })
                .ToList();

            // Lấy người dùng mới (5 người dùng mới nhất)
            RecentUsers = _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new RecentUserViewModel
                {
                    FullName = u.FullName,
                    Phone = u.Phone,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt ?? DateTime.MinValue
                })
                .ToList();
        }
    }

    public class RecentAppointmentViewModel
    {
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public string Status { get; set; }
    }

    public class RecentUserViewModel
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 