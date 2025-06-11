using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneTouch.Models;

namespace OneTouch.Pages.ScheduleManager
{
    public class CreateModel : PageModel
    {
        private readonly OneTouch.Models.OneTouchDbContext _context;

        public CreateModel(OneTouch.Models.OneTouchDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            var doctors = _context.Users
            .Where(u => "Doctor".Equals(u.Role))
            .Select(u => new {
                u.UserId,
                u.FullName
            })
            .ToList();

            ViewData["Doctor"] = new SelectList(doctors, "UserId", "FullName");
            return Page();
        }

        [BindProperty]
        public Schedule Schedule { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var doctor = await _context.Doctors.FirstOrDefaultAsync(u => u.UserId == Schedule.DoctorId);

            if (doctor == null)
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy bác sĩ phù hợp.");
                return Page();
            }

            Schedule.DoctorId = doctor.DoctorId;

            _context.Schedules.Add(Schedule);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
