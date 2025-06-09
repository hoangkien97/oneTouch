using Microsoft.EntityFrameworkCore;
using OneTouch.Models;
using OneTouch.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneTouch.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly OneTouchDbContext _context;

        public AppointmentRepository(OneTouchDbContext context)
        {
            _context = context;
        }

        public async Task<List<Appointment>> GetUpcomingAppointmentsByUserIdAsync(int userId)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Where(a => a.UserId == userId && a.CreatedAt >= DateTime.Today)
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Appointment> GetByIdAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);
        }

        public async Task<Appointment> CreateAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<Appointment> UpdateAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task DeleteAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }
        }
    }
} 