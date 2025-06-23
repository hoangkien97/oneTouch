using OneTouch.Models;
using System.Collections.Generic;
namespace OneTouch.Services
{
    public class ScheduleSeeder
    {
        /// <summary>
        /// Tạo danh sách Schedule mặc định cho một ngày
        /// </summary>
        public static List<Schedule> GenerateDoctorSchedulesForDate(DateOnly date, int doctorId)
        {
            var list = new List<Schedule>();
            // Chỉ làm từ Thứ 2 → Thứ 6
            if (date.DayOfWeek < DayOfWeek.Monday || date.DayOfWeek > DayOfWeek.Friday)
                return list;

            var start = new TimeOnly(7, 0);
            var end = new TimeOnly(17, 0);
            var lunchStart = new TimeOnly(12, 0);
            var lunchEnd = new TimeOnly(13, 0);

            while (start < end)
            {
                var next = start.AddMinutes(30);
                // Bỏ khung trưa
                if (!(start >= lunchStart && next <= lunchEnd))
                {
                    list.Add(new Schedule
                    {
                        Date = date,
                        StartTime = start,
                        EndTime = next,
                        DoctorId = doctorId,
                        MaxPatients = 1,
                        Appointments = null
                    });
                }
                start = next;
            }
            return list;
        }

        /// <summary>
        /// Tạo lịch mặc định cho toàn bộ 1 tháng (bỏ cuối tuần nếu cần)
        /// </summary>
        public static List<Schedule> GenerateDoctorSchedulesForMonth(int year, int month, int doctorId)
        {
            var all = new List<Schedule>();
            int daysInMonth = DateTime.DaysInMonth(year, month);
            for (int d = 1; d <= daysInMonth; d++)
            {
                var date = new DateOnly(year, month, d);
                all.AddRange(GenerateDoctorSchedulesForDate(date, doctorId));
            }
            return all;
        }
    }
}
