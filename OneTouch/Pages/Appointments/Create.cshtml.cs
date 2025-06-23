using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OneTouch.Models;
using OneTouch.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace OneTouch.Pages.Appointments
{
    public class CreateModel : PageModel
    {
        private readonly IAppointmentService _appointmentService;
        private readonly OneTouchDbContext _context;

        public CreateModel(IAppointmentService appointmentService, OneTouchDbContext context)
        {
            _appointmentService = appointmentService;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public List<Specialty> Specialties { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
        public int? PreselectedSpecialtyId { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng chọn lịch khám")]
            public int ScheduleId { get; set; }

            public int? DoctorId { get; set; }

            public string Note { get; set; }
        }

        public async Task<IActionResult> OnGetAsync([FromQuery] int? specialtyId)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToPage("/Account/Login");
            }

            // Load specialties
            Specialties = await _context.Specialties.ToListAsync();

            if (specialtyId.HasValue)
            {
                PreselectedSpecialtyId = specialtyId.Value;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Specialties = await _context.Specialties.ToListAsync();
                return Page();
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToPage("/Account/Login");
            }

            if (!int.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Account/Login");
            }

            // Check if schedule exists and has available slots
            var schedule = await _context.Schedules
                .Include(s => s.Appointments)
                .FirstOrDefaultAsync(s => s.ScheduleId == Input.ScheduleId);

            if (schedule == null)
            {
                ErrorMessage = "Lịch khám không tồn tại!";
                Specialties = await _context.Specialties.ToListAsync();
                return Page();
            }

            if (schedule.Appointments.Count(a => a.Status != "Cancelled") >= schedule.MaxPatients)
            {
                ErrorMessage = "Lịch khám này đã hết chỗ!";
                Specialties = await _context.Specialties.ToListAsync();
                return Page();
            }

            // Check if user already has an appointment at this time
            var existingAppointment = await _context.Appointments
                .Include(a => a.Schedule)
                .FirstOrDefaultAsync(a => a.UserId == userId && 
                                        a.Schedule.Date == schedule.Date &&
                                        a.Schedule.StartTime == schedule.StartTime &&
                                        a.Status != "Cancelled");

            if (existingAppointment != null)
            {
                ErrorMessage = "Bạn đã có lịch khám vào thời gian này!";
                Specialties = await _context.Specialties.ToListAsync();
                return Page();
            }

            // Create appointment
            var appointment = new Appointment
            {
                ScheduleId = Input.ScheduleId,
                UserId = userId,
                Status = "Pending",
                Note = Input.Note,
                CreatedAt = DateTime.Now
            };

            await _appointmentService.CreateAsync(appointment);

            SuccessMessage = "Đặt lịch khám thành công!";
            Specialties = await _context.Specialties.ToListAsync();
            return Page();
        }
    }
} 