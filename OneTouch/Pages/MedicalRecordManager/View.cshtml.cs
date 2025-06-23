using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OneTouch.Models;

namespace OneTouch.Pages.MedicalRecordManager
{
    public class ViewModel : PageModel
    {
        private readonly OneTouch.Models.OneTouchDbContext _context;

        public ViewModel(OneTouch.Models.OneTouchDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int UserId { get; set; }

        public string? PatientName { get; set; }

        public List<MedicalRecord> MedicalRecords { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Lấy bệnh nhân
            var user = await _context.Users.FindAsync(UserId);

            Console.WriteLine($"UserId: {UserId}");

            if (user == null)
                return NotFound("Không tìm thấy bệnh nhân.");

            PatientName = user.FullName;

            // Lấy danh sách MedicalRecords kèm thông tin bác sĩ & chuyên ngành
            MedicalRecords = await _context.MedicalRecords
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Schedule)
                        .ThenInclude(s => s.Doctor)
                            .ThenInclude(d => d.User)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Schedule)
                        .ThenInclude(s => s.Doctor)
                            .ThenInclude(d => d.Specialty)
                .Where(r => r.Appointment != null && r.Appointment.UserId == UserId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Page();
        }
    }
}

