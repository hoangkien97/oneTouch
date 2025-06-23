using Microsoft.AspNetCore.Mvc.RazorPages;
using OneTouch.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneTouch.Pages.Specialties
{
    public class IndexModel : PageModel
    {
        private readonly OneTouchDbContext _context;

        public IndexModel(OneTouchDbContext context)
        {
            _context = context;
        }

        public List<SpecialtyViewModel> Specialties { get; set; }

        public async Task OnGetAsync()
        {
            var specialtiesFromDb = await _context.Specialties.ToListAsync();
            
            Specialties = new List<SpecialtyViewModel>();
            foreach (var specialty in specialtiesFromDb)
            {
                Specialties.Add(new SpecialtyViewModel
                {
                    Specialty = specialty,
                    IconClass = GetIconForSpecialty(specialty.Name)
                });
            }
        }

        private string GetIconForSpecialty(string specialtyName)
        {
            return specialtyName?.ToLower() switch
            {
                "tim mạch" => "fas fa-heartbeat",
                "nhi khoa" => "fas fa-child",
                "da liễu" => "fas fa-spa",
                "tai - mũi - họng" => "fas fa-head-side-cough",
                "nội tổng quát" => "fas fa-stethoscope",
                "thần kinh" => "fas fa-brain",
                "tiêu hóa" => "fas fa-stomach",
                "cơ xương khớp" => "fas fa-bone",
                "cột sống" => "fas fa-person-dots-from-line",
                "sản phụ khoa" => "fas fa-baby-carriage",
                "y học cổ truyền" => "fas fa-mortar-pestle",
                "châm cứu" => "fas fa-hand-sparkles",
                "siêu âm thai" => "fas fa-wave-square",
                _ => "fas fa-clinic-medical",
            };
        }
    }

    public class SpecialtyViewModel
    {
        public Specialty Specialty { get; set; }
        public string IconClass { get; set; }
    }
} 