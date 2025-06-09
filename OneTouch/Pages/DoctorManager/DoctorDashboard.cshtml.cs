using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OneTouch.Models;

namespace OneTouch.Pages.DoctorManager
{
    public class DoctorDashboardModel : PageModel
    {
        private readonly OneTouch.Models.OneTouchDbContext _context;

        public List<Schedule> Schedules { get; set; } = new();

        public int DoctorId { get; set; }
        public DoctorDashboardModel(OneTouch.Models.OneTouchDbContext context) 
        {
            _context = context;
        }

        public async Task OnGetAsync(int doctorId)
        {
            DoctorId = doctorId;
            Schedules = await _context.Schedules
                .Include(s => s.Appointments)
                .Where(s => s.DoctorId == doctorId)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }
    }
}
