using OneTouch.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneTouch.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<List<Appointment>> GetUpcomingAppointmentsByUserIdAsync(int userId);
        Task<Appointment> GetByIdAsync(int id);
        Task<Appointment> CreateAsync(Appointment appointment);
        Task<Appointment> UpdateAsync(Appointment appointment);
        Task DeleteAsync(int id);
    }
} 