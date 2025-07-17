using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OneTouch.Models;

namespace OneTouch.Pages.DoctorManager
{
    public class PatientForDoctorManagerModel : PageModel
    {
        private readonly OneTouchDbContext _context;
        public PatientForDoctorManagerModel(OneTouchDbContext context)
        {
            _context = context;
        }

        public List<OneTouch.Models.User> Patients { get; set; } = new();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }

        private const int PageSize = 10;

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }


        [BindProperty(SupportsGet = true)]
        public int Page { get; set; } = 1;

        

        [BindProperty(SupportsGet = true)]
        public int DoctorId { get; set; }

        public async Task OnGetAsync(int doctorId)
        {
            DoctorId = doctorId;

            var patientIdsQuery = _context.Appointments
        .Where(a => a.Schedule != null && a.Schedule.DoctorId == doctorId
                 && a.User != null && a.User.Role == "patient")
        .Select(a => a.UserId)
        .Distinct();

            var userQuery = _context.Users
                .Where(u => patientIdsQuery.Contains(u.UserId));

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var term = SearchTerm.ToLower();
                userQuery = userQuery.Where(u =>
                    (u.FullName != null && u.FullName.ToLower().Contains(term)) ||
                    (u.Phone != null && u.Phone.ToLower().Contains(term)));
            }

            var totalCount = await userQuery.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            CurrentPage = Page;

            Patients = await userQuery
                .OrderBy(u => u.FullName)
                .Skip((Page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}

