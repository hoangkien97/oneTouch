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
    public class IndexModel : PageModel
    {
        private readonly OneTouch.Models.OneTouchDbContext _context;

        public IndexModel(OneTouch.Models.OneTouchDbContext context)
        {
            _context = context;
        }

        public IList<Schedule> Schedule { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Schedule = await _context.Schedules
                .Include(s => s.Doctor)
                .ThenInclude(d => d.User)
                .ToListAsync();
        }
    }
}