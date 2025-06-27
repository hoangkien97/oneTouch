using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OneTouch.Models;
using OneTouch.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;

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
        public int? PreselectedDoctorId { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng chọn lịch khám")]
            public int ScheduleId { get; set; }

            public int? DoctorId { get; set; }

            public string Note { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn hình thức thanh toán")]
            public string PaymentMethod { get; set; }
        }

        public async Task<IActionResult> OnGetAsync([FromQuery] int? specialtyId, [FromQuery] int? doctorId)
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
            if (doctorId.HasValue)
            {
                PreselectedDoctorId = doctorId.Value;
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
                .Include(s => s.Doctor)
                    .ThenInclude(d => d.Specialty)
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

            // Tính giá tiền theo loại khám
            var specialtyName = schedule?.Doctor?.Specialty?.Name?.Trim().ToLower() ?? "";
            decimal amount = 300000; // fallback mặc định chuyên khoa
            if (!string.IsNullOrEmpty(specialtyName) && (specialtyName.Contains("tổng quát") || specialtyName.Contains("tong quat")))
            {
                amount = 200000;
            }
            if (amount <= 0) amount = 200000; // Bảo vệ cuối cùng, không bao giờ gửi 0 sang VNPay
            System.Diagnostics.Debug.WriteLine($"[VNPay] specialtyName: {specialtyName}, amount: {amount}");

            if (Input.PaymentMethod == "VnPay")
            {
                // Chuyển hướng sang PaymentController để lấy URL VNPay
                var paymentInfo = new PaymentInformationModel
                {
                    OrderType = "appointment",
                    Amount = (double)amount,
                    OrderDescription = "Thanh toán đặt lịch khám",
                    Name = User.Identity.Name ?? "Khách hàng",
                    ScheduleId = Input.ScheduleId,
                    UserId = userId,
                    Note = Input.Note
                };
                
                // Log để debug
                System.Diagnostics.Debug.WriteLine($"[VNPay] PaymentInfo - Amount: {paymentInfo.Amount}, ScheduleId: {paymentInfo.ScheduleId}, UserId: {paymentInfo.UserId}");
                
                // Chuyển hướng trực tiếp với query string
                var queryParams = new List<string>
                {
                    $"OrderType={Uri.EscapeDataString(paymentInfo.OrderType)}",
                    $"Amount={paymentInfo.Amount:F0}",
                    $"OrderDescription={Uri.EscapeDataString(paymentInfo.OrderDescription)}",
                    $"Name={Uri.EscapeDataString(paymentInfo.Name)}",
                    $"ScheduleId={paymentInfo.ScheduleId}",
                    $"UserId={paymentInfo.UserId}",
                    $"Note={Uri.EscapeDataString(paymentInfo.Note ?? "")}"
                };
                
                var queryString = string.Join("&", queryParams);
                var redirectUrl = $"/Payment/CreatePaymentUrlVnpay?{queryString}";
                
                System.Diagnostics.Debug.WriteLine($"[VNPay] Redirect URL: {redirectUrl}");
                return Redirect(redirectUrl);
            }

            // Nếu chọn offline, tạo như cũ
            var appointment = new Appointment
            {
                ScheduleId = Input.ScheduleId,
                UserId = userId,
                Status = "Pending",
                Note = Input.Note,
                CreatedAt = DateTime.Now
            };

            await _appointmentService.CreateAsync(appointment);

            // Tạo invoice cho lịch khám này
            var invoice = new Invoice
            {
                AppointmentId = appointment.AppointmentId,
                TotalAmount = amount,
                PaymentStatus = "Unpaid",
                PaymentMethod = Input.PaymentMethod,
                CreatedAt = DateTime.Now
            };
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            SuccessMessage = "Đặt lịch khám thành công!";
            Specialties = await _context.Specialties.ToListAsync();
            return Page();
        }
    }
} 