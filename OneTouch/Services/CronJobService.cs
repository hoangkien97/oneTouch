using Cronos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OneTouch.Models;
using System;
namespace OneTouch.Services
{
    public class CronJobService : BackgroundService
    {
        private readonly ILogger<CronJobService> _logger;
        private readonly CronExpression _cronExpression;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeZoneInfo _timeZone;


        public CronJobService(ILogger<CronJobService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            // Biểu thức cron: chạy lúc 00:00 hàng ngày
            //_cronExpression = CronExpression.Parse("0 0 * * *"); // phút 0, giờ 0, mỗi ngày
            _cronExpression = CronExpression.Parse("23 09 * * *"); // phút 0, giờ 0, mỗi ngày
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // Việt Nam
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var next = _cronExpression.GetNextOccurrence(DateTimeOffset.Now, _timeZone);
                if (next.HasValue)
                {
                    var delay = next.Value - DateTimeOffset.Now;
                    if (delay.TotalMilliseconds > 0)
                        await Task.Delay(delay, stoppingToken);

                    await DoScheduledWork();
                }
            }
        }

        private async Task DoScheduledWork()
        {
            _logger.LogInformation("⏰ DoctorScheduleCronJob started at {time}", DateTime.Now);

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OneTouchDbContext>();
            var scheduleService = scope.ServiceProvider.GetRequiredService<DoctorScheduleService>();

            var doctorIds = await context.Doctors
                          .Select(d => d.DoctorId)
                          .ToListAsync();

            foreach (var doctorId in doctorIds)
            {
                await scheduleService.SeedSchedulesForDoctorAsync(doctorId);
            }

            _logger.LogInformation("✅ DoctorScheduleCronJob completed at {time}", DateTime.Now);
        }
    }
}
