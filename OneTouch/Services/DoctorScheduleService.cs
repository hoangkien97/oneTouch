using Microsoft.EntityFrameworkCore;
using OneTouch.Models;
using OneTouch.Services.Interfaces;

namespace OneTouch.Services
{
    public class DoctorScheduleService
    {
        private readonly OneTouchDbContext _context;
        private readonly ILogger<DoctorScheduleService> _logger;

        public DoctorScheduleService(OneTouchDbContext context,ILogger<DoctorScheduleService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task SeedSchedulesForDoctorAsync(int doctorId)
        {
            var today = DateTime.Today;
            var monthsToSeed = new[]
            {
            (today.Year, today.Month),
            (today.AddMonths(1).Year, today.AddMonths(1).Month)
        };

            foreach (var (year, month) in monthsToSeed)
            {
                var existingDates = await _context.Schedules
                    .Where(s => s.Date.HasValue &&
                                s.Date.Value.Year == year &&
                                s.Date.Value.Month == month &&
                                s.DoctorId == doctorId)
                    .Select(s => s.Date.Value)
                    .Distinct()
                    .ToListAsync();

                var candidates = ScheduleSeeder
                    .GenerateDoctorSchedulesForMonth(year, month, doctorId);

                var toInsert = candidates
                    .Where(s => !existingDates.Contains(s.Date.GetValueOrDefault()))
                    .ToList();

                if (toInsert.Any())
                {
                    _context.Schedules.AddRange(toInsert);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation(
                        "✅ Đã tạo {count} lịch cho bác sĩ {doc} tháng {m}/{y}",
                        toInsert.Count, doctorId, month, year);
                }
                else
                {
                    _logger.LogInformation(
                        "✅ Bác sĩ {doc} đã có lịch tháng {m}/{y}",
                        doctorId, month, year);
                }
            }
        }
    }
}
