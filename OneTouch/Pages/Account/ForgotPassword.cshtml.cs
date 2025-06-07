using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OneTouch.Models;
using OneTouch.Services;
using OneTouch.Services.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.AspNetCore.Identity;
using System.Text;
using System.Text.Encodings.Web;
using System.ComponentModel.DataAnnotations;

namespace OneTouch.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly OneTouchDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;

        public ForgotPasswordModel(OneTouchDbContext context, IEmailService emailService, ISmsService smsService)
        {
            _context = context;
            _emailService = emailService;
            _smsService = smsService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập email hoặc số điện thoại")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập email hoặc số điện thoại")]
            public string Phone { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Xóa validation mặc định cho Email và Phone
            ModelState.Remove("Input.Email");
            ModelState.Remove("Input.Phone");

            if (!ModelState.IsValid)
                return Page();

            // Kiểm tra xem người dùng đã nhập ít nhất một trong hai thông tin chưa
            if (string.IsNullOrWhiteSpace(Input.Email) && string.IsNullOrWhiteSpace(Input.Phone))
            {
                ErrorMessage = "Vui lòng nhập email hoặc số điện thoại!";
                return Page();
            }

            // Nếu có email, thử tìm user bằng email
            if (!string.IsNullOrWhiteSpace(Input.Email))
            {
                var userByEmail = _context.Users.FirstOrDefault(u => u.Email == Input.Email);
                if (userByEmail != null)
                {
                    // Generate password reset token
                    var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                    var tokenExpiry = DateTime.UtcNow.AddHours(1);

                    // Save token to database
                    userByEmail.PasswordResetToken = token;
                    userByEmail.PasswordResetTokenExpiry = tokenExpiry;
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
                        <p>Xin chào {userByEmail.FullName},</p>
                        <p>Bạn đã yêu cầu đặt lại mật khẩu. Vui lòng click vào link bên dưới để đặt lại mật khẩu:</p>
                        <p><a href='{HtmlEncoder.Default.Encode(resetLink)}'>Đặt lại mật khẩu</a></p>
                        <p>Link này sẽ hết hạn sau 1 giờ.</p>
                        <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>";

                    await _emailService.SendEmailAsync(Input.Email, "Đặt lại mật khẩu - OneTouch", emailBody);
                    SuccessMessage = "Link đặt lại mật khẩu đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư.";
                    return Page();
                }
            }

            // Nếu có số điện thoại, thử tìm user bằng số điện thoại
            if (!string.IsNullOrWhiteSpace(Input.Phone))
            {
                var userByPhone = _context.Users.FirstOrDefault(u => u.Phone == Input.Phone);
                if (userByPhone != null)
                {
                    // Generate OTP
                    //var otp = new Random().Next(100000, 999999).ToString();
                    var otp = "123456";

                    var otpExpiry = DateTime.UtcNow.AddMinutes(5);

                    // Save OTP to database
                    userByPhone.PasswordResetToken = otp;
                    userByPhone.PasswordResetTokenExpiry = otpExpiry;
                    await _context.SaveChangesAsync();

                    // Send OTP via SMS
                    var sent = await _smsService.SendOtpAsync(Input.Phone, otp);
                    if (!sent)
                    {
                        ErrorMessage = "Không thể gửi mã OTP. Vui lòng thử lại sau!";
                        return Page();
                    }

                    // Chuyển hướng đến trang xác thực OTP
                    return RedirectToPage("/Account/VerifyOtp", new { phone = Input.Phone });
                }
            }

            // Nếu không tìm thấy user với cả email và số điện thoại
            ErrorMessage = "Không tìm thấy tài khoản với thông tin đã cung cấp!";
            return Page();
        }
    }
} 