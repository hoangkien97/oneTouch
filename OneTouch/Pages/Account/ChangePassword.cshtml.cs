using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OneTouch.Models;
using OneTouch.Services;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace OneTouch.Pages.Account
{
    public class ChangePasswordModel : PageModel
    {
        private readonly OneTouchDbContext _context;

        public ChangePasswordModel(OneTouchDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public class InputModel
        {
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
            public string ConfirmPassword { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Account/Login");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Kiểm tra mật khẩu cũ
            if (!PasswordService.VerifyPassword(Input.CurrentPassword, user.PasswordHash))
            {
                ErrorMessage = "Mật khẩu hiện tại không đúng!";
                return Page();
            }

            // Kiểm tra mật khẩu mới hợp lệ
            if (!ValidationService.IsValidPassword(Input.NewPassword))
            {
                ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự!";
                return Page();
            }

            if (Input.NewPassword != Input.ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu xác nhận không khớp!";
                return Page();
            }

            // Không cho phép trùng mật khẩu cũ
            if (PasswordService.VerifyPassword(Input.NewPassword, user.PasswordHash))
            {
                ErrorMessage = "Mật khẩu mới không được trùng với mật khẩu hiện tại!";
                return Page();
            }

            // Cập nhật mật khẩu mới
            user.PasswordHash = PasswordService.HashPassword(Input.NewPassword);
            await _context.SaveChangesAsync();

            SuccessMessage = "Đổi mật khẩu thành công!";
            return Page();
        }
    }
} 