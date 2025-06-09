using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OneTouch.Models;
using OneTouch.Services;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace OneTouch.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly OneTouchDbContext _context;

        public ResetPasswordModel(OneTouchDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public class InputModel
        {
            public string Email { get; set; }
            public string Token { get; set; }
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
            public string Code { get; set; } 
        }

        public IActionResult OnGet(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Account/Login");
            }

            Input = new InputModel
            {
                Email = email,
                Token = token
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Validate password
            if (!ValidationService.IsValidPassword(Input.Password))
            {
                ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự!";
                return Page();
            }

            if (Input.Password != Input.ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu xác nhận không khớp!";
                return Page();
            }

            // Find user and validate token
            var user = _context.Users.FirstOrDefault(u => u.Email == Input.Email);
            if (user == null)
            {
                ErrorMessage = "Không tìm thấy tài khoản!";
                return Page();
            }

            if (user.PasswordResetToken != Input.Token)
            {
                ErrorMessage = "Token không hợp lệ!";
                return Page();
            }

            if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                ErrorMessage = "Token đã hết hạn! Vui lòng yêu cầu đặt lại mật khẩu mới.";
                return Page();
            }

            // Update password
            user.PasswordHash = PasswordService.HashPassword(Input.Password);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await _context.SaveChangesAsync();

            SuccessMessage = "Mật khẩu đã được đặt lại thành công! Bạn có thể đăng nhập bằng mật khẩu mới.";
            return Page();
        }
    }
} 