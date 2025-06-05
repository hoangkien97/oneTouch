using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OneTouch.Models;
using OneTouch.Services;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.AspNetCore.Identity;
using System.Text;
using System.Text.Encodings.Web;
using OneTouch.Services.Interfaces;

namespace OneTouch.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly OneTouchDbContext _context;
        private readonly IEmailService _emailService;

        public ForgotPasswordModel(OneTouchDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public class InputModel
        {
            public string Email { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Validate email
            if (!ValidationService.IsValidEmail(Input.Email))
            {
                ErrorMessage = "Email không hợp lệ!";
                return Page();
            }

            // Check if user exists
            var user = _context.Users.FirstOrDefault(u => u.Email == Input.Email);
            if (user == null)
            {
                ErrorMessage = "Không tìm thấy tài khoản với email này!";
                return Page();
            }

            // Generate password reset token
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var tokenExpiry = DateTime.UtcNow.AddHours(24);

            // Save token to database
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = tokenExpiry;
            await _context.SaveChangesAsync();

            // Generate reset link
            var resetLink = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { email = Input.Email, token = token },
                protocol: Request.Scheme);

            // Send email
            var emailBody = $@"
                <h2>Đặt lại mật khẩu</h2>
                <p>Xin chào {user.FullName},</p>
                <p>Bạn đã yêu cầu đặt lại mật khẩu. Vui lòng click vào link bên dưới để đặt lại mật khẩu:</p>
                <p><a href='{HtmlEncoder.Default.Encode(resetLink)}'>Đặt lại mật khẩu</a></p>
                <p>Link này sẽ hết hạn sau 24 giờ.</p>
                <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>";

            await _emailService.SendEmailAsync(Input.Email, "Đặt lại mật khẩu - OneTouch", emailBody);

            SuccessMessage = "Link đặt lại mật khẩu đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư.";
            return Page();
        }
    }
} 