using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using OneTouch.Models;

namespace OneTouch.Pages.AppointmentManager
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
            var schedules = _context.Schedules
                .Where(s => s.Date != null && s.StartTime != null && s.EndTime != null)
                .Select(s => new
                {
                    s.ScheduleId,
                    DisplayText =
                        s.Date.Value.ToDateTime(TimeOnly.MinValue).ToString("dd/MM/yyyy") + " | " +
                        s.StartTime.Value.ToString("hh\\:mm") + " - " +
                        s.EndTime.Value.ToString("hh\\:mm")
                })
                .ToList();

            ViewData["Schedules"] = new SelectList(schedules, "ScheduleId", "DisplayText");

            var user = _context.Users.Where(u => "Patient".Equals(u.Role));
            ViewData["Users"] = new SelectList(user, "UserId", "FullName");
            return Page();
        }



        [BindProperty]
        public Appointment Appointment { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Appointments.Add(Appointment);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
