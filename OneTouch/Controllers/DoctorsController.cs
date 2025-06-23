using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneTouch.Models;
using System.Linq;
using System.Threading.Tasks;

namespace OneTouch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly OneTouchDbContext _context;

        public DoctorsController(OneTouchDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctors([FromQuery] int? specialtyId)
        {
            var query = _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Where(d => d.User != null);

            if (specialtyId.HasValue)
            {
                query = query.Where(d => d.SpecialtyId == specialtyId);
            }

            var doctors = await query
                .Select(d => new
                {
                    d.DoctorId,
                    d.User.FullName,
                    d.User.Email,
                    d.ExperienceYears,
                    d.Description,
                    SpecialtyName = d.Specialty.Name,
                    d.AvatarPath
                })
                .ToListAsync();

            return Ok(doctors);
        }
    }
} 