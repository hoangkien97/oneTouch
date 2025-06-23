using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneTouch.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OneTouch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulesController : ControllerBase
    {
        private readonly OneTouchDbContext _context;

        public SchedulesController(OneTouchDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetSchedules([FromQuery] string date, [FromQuery] int? doctorId)
        {
            if (!DateOnly.TryParse(date, out var scheduleDate))
            {
                return BadRequest("Invalid date format");
            }

            var query = _context.Schedules
                .Include(s => s.Appointments)
                .Include(s => s.Doctor)
                .ThenInclude(d => d.User)
                .Include(s => s.Doctor)
                .ThenInclude(d => d.Specialty)
                .Where(s => s.Date == scheduleDate);

            if (doctorId.HasValue)
            {
                query = query.Where(s => s.DoctorId == doctorId);
            }

            var schedules = await query
                .Select(s => new
                {
                    s.ScheduleId,
                    s.DoctorId,
                    DoctorName = s.Doctor.User.FullName,
                    SpecialtyName = s.Doctor.Specialty.Name,
                    StartTime = s.StartTime.Value.ToString("HH:mm"),
                    EndTime = s.EndTime.Value.ToString("HH:mm"),
                    s.MaxPatients,
                    CurrentPatients = s.Appointments.Count(a => a.Status != "Cancelled")
                })
                .ToListAsync();

            return Ok(schedules);
        }
    }
} 