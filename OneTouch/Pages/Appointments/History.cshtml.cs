using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OneTouch.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace OneTouch.Pages.Appointments
{
    public class HistoryModel : PageModel
    {
        private readonly OneTouchDbContext _context;
        public HistoryModel(OneTouchDbContext context)
        {
            _context = context;
        }
        public List<Appointment> Appointments { get; set; }
        [TempData] public string SuccessMessage { get; set; }
        [TempData] public string ErrorMessage { get; set; }
        [BindProperty]
        public int FeedbackRating { get; set; }
        [BindProperty]
        public string FeedbackComment { get; set; }
        [BindProperty]
        public int FeedbackAppointmentId { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userIdString) || string.IsNullOrEmpty(userRole))
            {
                return RedirectToPage("/Account/Login");
            }

            if (!int.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Account/Login");
            }

            Appointments = await _context.Appointments
                .Include(a => a.Schedule)
                    .ThenInclude(s => s.Doctor)
                        .ThenInclude(d => d.User)
                .Include(a => a.Feedbacks)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Schedule.Date)
                .ThenBy(a => a.Schedule.StartTime)
                .ToListAsync();
            return Page();
        }
        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userIdString) || string.IsNullOrEmpty(userRole))
            {
                return RedirectToPage("/Account/Login");
            }

            if (!int.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Account/Login");
            }

            var appt = await _context.Appointments
                .Include(a => a.Schedule)
                .FirstOrDefaultAsync(a => a.AppointmentId == id && a.UserId == userId);
            if (appt == null)
            {
                ErrorMessage = "Không tìm thấy lịch hẹn.";
                return RedirectToPage();
            }
            if (appt.Status == "Cancelled")
            {
                ErrorMessage = "Lịch hẹn đã bị hủy trước đó.";
                return RedirectToPage();
            }
            appt.Status = "Cancelled";
            await _context.SaveChangesAsync();
            SuccessMessage = "Đã hủy lịch thành công.";
            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostFeedbackAsync()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Account/Login");
            }
            var appt = await _context.Appointments.Include(a => a.Feedbacks).FirstOrDefaultAsync(a => a.AppointmentId == FeedbackAppointmentId && a.UserId == userId);
            if (appt == null)
            {
                ErrorMessage = "Không tìm thấy lịch hẹn để feedback.";
                return RedirectToPage();
            }
            if (appt.Feedbacks.Any())
            {
                ErrorMessage = "Bạn đã gửi feedback cho lịch này rồi.";
                return RedirectToPage();
            }
            var feedback = new Feedback
            {
                AppointmentId = appt.AppointmentId,
                Rating = FeedbackRating,
                Comment = FeedbackComment,
                CreatedAt = DateTime.Now
            };
            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();
            SuccessMessage = "Gửi feedback thành công!";
            return RedirectToPage();
        }
    }
} 