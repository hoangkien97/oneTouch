using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OneTouch.Models;

namespace OneTouch.Pages.AppointmentManager
{
    public class IndexModel : PageModel
    {
        private readonly OneTouch.Models.OneTouchDbContext _context;

        public IndexModel(OneTouch.Models.OneTouchDbContext context)
        {
            _context = context;
        }

        public IList<Appointment> Appointment { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Appointment = await _context.Appointments
                .Include(a => a.Schedule)
                .Include(a => a.User).ToListAsync();
        }
    }
}
