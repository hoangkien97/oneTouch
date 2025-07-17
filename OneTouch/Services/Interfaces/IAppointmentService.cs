using OneTouch.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneTouch.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<List<Appointment>> GetUpcomingAppointmentsAsync(int userId);
        Task<Appointment> GetByIdAsync(int id);
        Task<Appointment> CreateAsync(Appointment appointment);
        Task<Appointment> UpdateAsync(Appointment appointment);
        Task DeleteAsync(int id);
    }
} 