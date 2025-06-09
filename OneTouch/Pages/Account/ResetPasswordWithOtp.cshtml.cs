using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OneTouch.Models;
using OneTouch.Services;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.ComponentModel.DataAnnotations;

namespace OneTouch.Pages.Account
{
    public class ResetPasswordWithOtpModel : PageModel
    {
        private readonly OneTouchDbContext _context;

        public ResetPasswordWithOtpModel(OneTouchDbContext context)
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
            public string Token { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
            [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {6} ký tự.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string phone, string token)
        {
            Input = new InputModel
            {
                Phone = phone,
                Token = token
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = _context.Users.FirstOrDefault(u => u.Phone == Input.Phone);
            if (user == null)
            {
                ErrorMessage = "Không tìm thấy tài khoản!";
                return Page();
            }

            // Kiểm tra token và thời gian hết hạn
            if (user.PasswordResetToken != Input.Token || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                ErrorMessage = "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn!";
                return Page();
            }

            // Cập nhật mật khẩu mới
            user.PasswordHash = PasswordService.HashPassword(Input.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await _context.SaveChangesAsync();

            // Hiển thị thông báo thành công
            SuccessMessage = "Mật khẩu đã được đặt lại thành công! Bạn có thể đăng nhập bằng mật khẩu mới.";

            return Page(); 
        }

    }
} 