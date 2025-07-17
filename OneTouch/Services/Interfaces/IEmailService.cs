using System.Threading.Tasks;
using OneTouch.Models;

namespace OneTouch.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task SendAppointmentConfirmationEmailAsync(Appointment appointment, Invoice invoice, string transactionId);
    }
} 