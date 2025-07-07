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

            var slots = new List<(TimeOnly start, TimeOnly end)>
            {
                (new TimeOnly(7,30), new TimeOnly(9,30)),
                (new TimeOnly(9,30), new TimeOnly(11,30)),
                (new TimeOnly(13,30), new TimeOnly(15,30)),
                (new TimeOnly(15,30), new TimeOnly(17,30))
            };

            foreach (var slot in slots)
            {
                list.Add(new Schedule
                {
                    Date = date,
                    StartTime = slot.start,
                    EndTime = slot.end,
                    DoctorId = doctorId,
                    MaxPatients = 5,
                    Appointments = null
                });
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
