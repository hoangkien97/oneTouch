using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneTouch.Models;

namespace OneTouch.Pages.MedicalRecordManager
{
    public class CreateModel : PageModel
    {
        private readonly OneTouch.Models.OneTouchDbContext _context;

        public CreateModel(OneTouch.Models.OneTouchDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int UserId { get; set; }

        public string? PatientName { get; set; }

        [BindProperty]
        public MedicalRecord MedicalRecord { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int DoctorId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var latestAppointment = await _context.Appointments
                .Include(a => a.User)
                .Where(a => a.UserId == UserId)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();

            if (latestAppointment == null)
                return NotFound("Không tìm thấy cuộc hẹn nào cho bệnh nhân này.");

            PatientName = latestAppointment.User?.FullName ?? "Không rõ";

            // Luôn gán thông tin cho MedicalRecord mới
            MedicalRecord.AppointmentId = latestAppointment.AppointmentId;
            MedicalRecord.CreatedAt = DateTime.Now;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            MedicalRecord.CreatedAt = DateTime.Now;
            _context.MedicalRecords.Add(MedicalRecord);

            await _context.SaveChangesAsync();

            var doctorId = await _context.Appointments
                .Where(a => a.AppointmentId == MedicalRecord.AppointmentId)
                .Select(a => a.Schedule!.DoctorId)
                .FirstOrDefaultAsync();

            return RedirectToPage("/DoctorManager/PatientForDoctorManager", new { doctorId });
        }

    }
}
