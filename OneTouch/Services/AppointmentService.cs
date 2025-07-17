using OneTouch.Models;
using OneTouch.Repositories.Interfaces;
using OneTouch.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneTouch.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;

        public AppointmentService(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<List<Appointment>> GetUpcomingAppointmentsAsync(int userId)
        {
            return await _appointmentRepository.GetUpcomingAppointmentsByUserIdAsync(userId);
        }

        public async Task<Appointment> GetByIdAsync(int id)
        {
            return await _appointmentRepository.GetByIdAsync(id);
        }

        public async Task<Appointment> CreateAsync(Appointment appointment)
        {
            return await _appointmentRepository.CreateAsync(appointment);
        }

        public async Task<Appointment> UpdateAsync(Appointment appointment)
        {
            return await _appointmentRepository.UpdateAsync(appointment);
        }

        public async Task DeleteAsync(int id)
        {
            await _appointmentRepository.DeleteAsync(id);
        }
    }
} 