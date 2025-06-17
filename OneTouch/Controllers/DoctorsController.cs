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
            if (!specialtyId.HasValue)
            {
                return BadRequest("Specialty ID is required");
            }

            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Where(d => d.SpecialtyId == specialtyId)
                .Select(d => new
                {
                    d.DoctorId,
                    FullName = d.User.FullName,
                    Specialty = d.Specialty.Name
                })
                .ToListAsync();

            return Ok(doctors);
        }
    }
} 