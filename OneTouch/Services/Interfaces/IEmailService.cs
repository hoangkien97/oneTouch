using System.Threading.Tasks;

namespace OneTouch.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
} 