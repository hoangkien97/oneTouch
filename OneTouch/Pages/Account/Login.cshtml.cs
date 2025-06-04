using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OneTouch.Models;
using OneTouch.Services;
using System.Threading.Tasks;
using System.Linq;

namespace OneTouch.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly OneTouchDbContext _context;

        public LoginModel(OneTouchDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ErrorMessage { get; set; }

        public class InputModel
        {
            public string Phone { get; set; }
            public string Password { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Validate các trường input
            if (!ValidationService.IsValidPhoneNumber(Input.Phone))
            {
                ErrorMessage = "Số điện thoại không hợp lệ! Số điện thoại phải có đúng 10 chữ số.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Input.Password))
            {
                ErrorMessage = "Vui lòng nhập mật khẩu!";
                return Page();
            }

            var user = _context.Users.FirstOrDefault(u => u.Phone == Input.Phone);

            if (user == null || !PasswordService.VerifyPassword(Input.Password, user.PasswordHash))
            {
                ErrorMessage = "Số điện thoại hoặc mật khẩu không đúng!";
                return Page();
            }

            // Đăng nhập thành công: Lưu thông tin vào session
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("UserName", user.FullName ?? user.Phone);
            HttpContext.Session.SetString("UserRole", user.Role ?? "patient");

            // Chuyển hướng về trang chủ user
            return RedirectToPage("/User/Home");
        }
    }
}