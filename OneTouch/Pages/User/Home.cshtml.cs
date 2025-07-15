using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OneTouch.Models;
using OneTouch.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace OneTouch.Pages.User
{
    public class HomeModel : PageModel
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IUserService _userService;
        private readonly OneTouchDbContext _context;

        public HomeModel(
            IAppointmentService appointmentService,
            IUserService userService,
            OneTouchDbContext context)
        {
            _appointmentService = appointmentService;
            _userService = userService;
            _context = context;
        }

        public string UserName { get; set; }
        public List<Appointment> UpcomingAppointments { get; set; }
        public List<DoctorViewModel> TopDoctors { get; set; }
        public List<TestimonialViewModel> Testimonials { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToPage("/Account/Login");
            }

            if (!int.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Account/Login");
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            UserName = user.FullName;

            // Get upcoming appointments
            UpcomingAppointments = await _appointmentService.GetUpcomingAppointmentsAsync(userId);

            // Get Top 3 Rated Doctors
            TopDoctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Include(d => d.Schedules)
                    .ThenInclude(s => s.Appointments)
                        .ThenInclude(a => a.Feedbacks)
                .Where(d => d.User != null && d.Schedules.SelectMany(s => s.Appointments).SelectMany(a => a.Feedbacks).Any(f => f.Rating.HasValue))
                .Select(d => new DoctorViewModel
                {
                    DoctorId = d.DoctorId,
                    FullName = d.User.FullName,
                    SpecialtyName = d.Specialty.Name,
                    AvatarPath = d.AvatarPath,
                    Description = d.Description,
                    AverageRating = d.Schedules
                                        .SelectMany(s => s.Appointments)
                                        .SelectMany(a => a.Feedbacks)
                                        .Average(f => f.Rating.Value),
                    RatingCount = d.Schedules
                                        .SelectMany(s => s.Appointments)
                                        .SelectMany(a => a.Feedbacks)
                                        .Count(f => f.Rating.HasValue)
                })
                .OrderByDescending(d => d.AverageRating)
                .ThenByDescending(d => d.RatingCount)
                .Take(3)
                .ToListAsync();

            // Lấy 5 feedback mới nhất có comment và rating, kèm tên khách hàng
            Testimonials = await _context.Feedbacks
                .Where(f => f.Rating.HasValue && !string.IsNullOrEmpty(f.Comment))
                .OrderByDescending(f => f.CreatedAt)
                .Take(5)
                .Select(f => new TestimonialViewModel
                {
                    CustomerName = f.Appointment.User.FullName ?? ("Khách hàng " + f.Appointment.UserId),
                    AvatarPath = "/uploads/hero/Icon1T.jpg",
                    Comment = f.Comment,
                    Rating = f.Rating.Value
                })
                .ToListAsync();

            return Page();
        }
    }

    public class TestimonialViewModel
    {
        public string CustomerName { get; set; }
        public string AvatarPath { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
    }
} 