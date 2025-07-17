using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OneTouch.Models;
using System.Globalization;

namespace OneTouch.Pages.DoctorManager
{
    public class DoctorDashboard2Model : PageModel
    {
        private readonly OneTouchDbContext _context;

        public List<Schedule> WeeklySchedules { get; set; } = new();
        public List<DateOnly> Weekdays { get; set; } = new(); // ngày cố định từ thứ 2 đến thứ 6
        public int DoctorId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedWeek { get; set; }

        public DoctorDashboard2Model(OneTouchDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync(int doctorId)
        {
            DoctorId = doctorId;

            // Nếu không có tuần được chọn, mặc định là tuần hiện tại
            if (string.IsNullOrEmpty(SelectedWeek))
            {
                var currentDate = DateTime.Today;
                var calendar = CultureInfo.InvariantCulture.Calendar;
                int weekNumber = calendar.GetWeekOfYear(currentDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                int currentYear = currentDate.Year;

                SelectedWeek = $"{currentYear}-W{weekNumber:D2}";


            }

            var parts = SelectedWeek.Split("-W");
            if (parts.Length != 2 || !int.TryParse(parts[0], out int parsedYear) || !int.TryParse(parts[1], out int week))
            {
                WeeklySchedules = new();
                return;
            }

            DateTime startOfWeek = FirstDateOfWeekISO8601(parsedYear, week);
            DateTime endOfWeek = startOfWeek.AddDays(6);

            Console.WriteLine($"SelectedWeek: {SelectedWeek}, startOfWeek: {startOfWeek:dd/MM/yyyy}");

            // Tạo danh sách ngày từ Thứ 2 đến Thứ 6
            Weekdays = Enumerable.Range(0, 5)
                .Select(i => DateOnly.FromDateTime(startOfWeek.AddDays(i)))
                .ToList();

            foreach (var d in Weekdays)
            {
                Console.WriteLine($"Weekday: {d}, DayOfWeek: {d.DayOfWeek}");
            }

            // Lấy tất cả schedule trong tuần (Thứ 2 -> CN), lọc thứ 2 -> 6 sau
            var allWeekSchedules = await _context.Schedules
                .Include(s => s.Appointments)
                .Where(s => s.DoctorId == doctorId
                            && s.Date >= DateOnly.FromDateTime(startOfWeek)
                            && s.Date <= DateOnly.FromDateTime(endOfWeek))
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            // Lọc lại chỉ giữ lịch thuộc Thứ 2 đến Thứ 6
            WeeklySchedules = allWeekSchedules
                .Where(s => Weekdays.Contains(s.Date.Value))
                .ToList();

            foreach (var s in WeeklySchedules)
            {
                Console.WriteLine($"Schedule: {s.Date}, DayOfWeek: {s.Date?.DayOfWeek}");
            }
        }

        // Trả về ngày thứ 2 đầu tiên của tuần theo ISO-8601 (Monday-start)
        private static DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            int firstWeek = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            int adjustedWeek = weekOfYear;
            if (firstWeek <= 1)
            {
                adjustedWeek -= 1;
            }

            DateTime result = firstThursday.AddDays(adjustedWeek * 7);
            return result.AddDays(-3); // quay về thứ 2
        }
    }
}
