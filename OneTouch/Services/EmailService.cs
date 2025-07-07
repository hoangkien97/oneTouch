using System.Net.Mail;
using System.Threading.Tasks;
using OneTouch.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using OneTouch.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace OneTouch.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly OneTouchDbContext _context;

        public EmailService(IConfiguration configuration, OneTouchDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpServer = _configuration["Email:SmtpServer"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
            var smtpUsername = _configuration["Email:SmtpUsername"];
            var smtpPassword = _configuration["Email:SmtpPassword"];
            var fromEmail = _configuration["Email:FromEmail"];
            var fromName = _configuration["Email:FromName"];

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            message.To.Add(email);

            await client.SendMailAsync(message);
        }

        public async Task SendAppointmentConfirmationEmailAsync(Appointment appointment, Invoice invoice, string transactionId)
        {
            // Lấy thông tin chi tiết appointment, bao gồm cả Doctor.User và Doctor.Specialty
            var appointmentDetails = await _context.Appointments
                .Include(a => a.Schedule)
                    .ThenInclude(s => s.Doctor)
                        .ThenInclude(d => d.User)
                .Include(a => a.Schedule)
                    .ThenInclude(s => s.Doctor)
                        .ThenInclude(d => d.Specialty)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointment.AppointmentId);

            if (appointmentDetails == null || appointmentDetails.User == null)
            {
                System.Diagnostics.Debug.WriteLine($"[EmailService] ERROR: Could not find appointment details for ID: {appointment.AppointmentId}");
                return;
            }

            var userEmail = appointmentDetails.User.Email;
            if (string.IsNullOrEmpty(userEmail))
            {
                System.Diagnostics.Debug.WriteLine($"[EmailService] ERROR: User email is null for appointment ID: {appointment.AppointmentId}");
                return;
            }

            var subject = "Xác nhận đặt lịch khám - OneTouch";
            var htmlMessage = GenerateAppointmentConfirmationEmail(appointmentDetails, invoice, transactionId);

            await SendEmailAsync(userEmail, subject, htmlMessage);
            
            System.Diagnostics.Debug.WriteLine($"[EmailService] SUCCESS: Sent confirmation email to {userEmail} for appointment ID: {appointment.AppointmentId}");
        }

        private string GenerateAppointmentConfirmationEmail(Appointment appointment, Invoice invoice, string transactionId)
        {
            var schedule = appointment.Schedule;
            var doctor = schedule?.Doctor;
            var specialty = doctor?.Specialty;
            var user = appointment.User;

            // Lấy thông tin bác sĩ giống như trong lịch sử khám
            var doctorName = doctor?.User?.FullName ?? "Chưa xác định";
            var doctorEmail = doctor?.User?.Email ?? "";
            var doctorPhone = doctor?.User?.Phone ?? "";
            var doctorSpecialty = specialty?.Name ?? "Chưa xác định";

            // Nếu có bác sĩ thì hiển thị, không thì ẩn hoàn toàn
            var doctorInfoHtml = "";
            if (doctorName != "Chưa xác định")
            {
                doctorInfoHtml = $@"
                    <p><strong>Bác sĩ:</strong> {doctorName}</p>
                    <p><strong>Email bác sĩ:</strong> {doctorEmail}</p>
                    <p><strong>Điện thoại bác sĩ:</strong> {doctorPhone}</p>
                    <p><strong>Chuyên khoa:</strong> {doctorSpecialty}</p>
                ";
            }

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Xác nhận đặt lịch khám</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 20px; border-radius: 0 0 5px 5px; }}
        .appointment-info {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 5px; border-left: 4px solid #007bff; }}
        .invoice-info {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 5px; border-left: 4px solid #28a745; }}
        .success-message {{ background-color: #d4edda; color: #155724; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6c757d; font-size: 14px; }}
        .highlight {{ color: #007bff; font-weight: bold; }}
        .amount {{ color: #28a745; font-weight: bold; font-size: 18px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Đặt lịch khám thành công!</h1>
            <p>OneTouch - Hệ thống đặt lịch khám trực tuyến</p>
        </div>
        
        <div class='content'>
            <div class='success-message'>
                <h3>✅ Thanh toán đã được xác nhận</h3>
                <p>Cảm ơn bạn đã sử dụng dịch vụ của OneTouch. Lịch khám của bạn đã được xác nhận và thanh toán thành công.</p>
            </div>

            <div class='appointment-info'>
                <h3>📅 Thông tin lịch khám</h3>
                <p><strong>Mã lịch khám:</strong> <span class='highlight'>#{appointment.AppointmentId}</span></p>
                <p><strong>Ngày khám:</strong> {schedule?.Date:dd/MM/yyyy}</p>
                <p><strong>Thời gian:</strong> {schedule?.StartTime:HH:mm} - {schedule?.EndTime:HH:mm}</p>
                {doctorInfoHtml}
                <p><strong>Ghi chú:</strong> {appointment.Note ?? "Không có"}</p>
            </div>

            <div class='invoice-info'>
                <h3>💰 Thông tin hóa đơn</h3>
                <p><strong>Mã hóa đơn:</strong> <span class='highlight'>#{invoice.InvoiceId}</span></p>
                <p><strong>Mã giao dịch:</strong> <span class='highlight'>{transactionId}</span></p>
                <p><strong>Số tiền:</strong> <span class='amount'>{invoice.TotalAmount:N0} VNĐ</span></p>
                <p><strong>Phương thức thanh toán:</strong> {invoice.PaymentMethod}</p>
                <p><strong>Trạng thái:</strong> <span style='color: #28a745; font-weight: bold;'>Đã thanh toán</span></p>
                <p><strong>Ngày thanh toán:</strong> {invoice.CreatedAt:dd/MM/yyyy HH:mm}</p>
            </div>

            <div style='background-color: #fff3cd; color: #856404; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                <h4>📋 Lưu ý quan trọng:</h4>
                <ul>
                    <li>Vui lòng đến trước giờ hẹn 15 phút để làm thủ tục</li>
                    <li>Mang theo giấy tờ tùy thân và thẻ bảo hiểm (nếu có)</li>
                    <li>Nếu cần hủy lịch, vui lòng liên hệ ít nhất 24 giờ trước</li>
                    <li>Mọi thắc mắc vui lòng liên hệ: <strong>+84973418074 (Ms Hương)</strong></li>
                </ul>
            </div>
        </div>

        <div class='footer'>
            <p>© 2024 OneTouch. Tất cả quyền được bảo lưu.</p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
} 