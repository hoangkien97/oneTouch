using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OneTouch.Models;

namespace OneTouch.Pages.MedicalRecordManager
{
    public class IndexModel : PageModel
    {
        private readonly OneTouch.Models.OneTouchDbContext _context;

        public IndexModel(OneTouch.Models.OneTouchDbContext context)
        {
            _context = context;
        }

        public IList<MedicalRecord> MedicalRecord { get; set; } = default!;

        public async Task OnGetAsync()
        {
            MedicalRecord = await _context.MedicalRecords
                .Include(m => m.Appointment).ToListAsync();
        }
    }
}
