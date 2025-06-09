using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OneTouch.Models;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace OneTouch.Pages.Account
{
    public class VerifyOtpModel : PageModel
    {
        private readonly OneTouchDbContext _context;

        public VerifyOtpModel(OneTouchDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public class InputModel
        {
            public string Phone { get; set; }
            public string Otp { get; set; }
        }

        public void OnGet(string phone)
        {
            Input = new InputModel { Phone = phone };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (string.IsNullOrWhiteSpace(Input.Otp) || Input.Otp.Length != 6)
            {
                ErrorMessage = "Mã OTP không hợp lệ!";
                return Page();
            }

            var user = _context.Users.FirstOrDefault(u => u.Phone == Input.Phone);
            if (user == null)
            {
                ErrorMessage = "Không tìm thấy tài khoản!";
                return Page();
            }

            // Kiểm tra OTP và thời gian hết hạn
            if (user.PasswordResetToken != Input.Otp || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                ErrorMessage = "Mã OTP không đúng hoặc đã hết hạn!";
                return Page();
            }

            // Tạo token reset password mới
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var tokenExpiry = DateTime.UtcNow.AddHours(1);

            // Lưu token mới vào database
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = tokenExpiry;
            await _context.SaveChangesAsync();

            // Chuyển hướng đến trang đặt lại mật khẩu mới
            return RedirectToPage("/Account/ResetPasswordWithOtp", new { phone = Input.Phone, token = token });
        }
    }
} 