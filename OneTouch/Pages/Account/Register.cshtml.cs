using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OneTouch.Models;
using OneTouch.Services;
using System.Threading.Tasks;
using System.Linq;

namespace OneTouch.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly OneTouchDbContext _context;

        public RegisterModel(OneTouchDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ErrorMessage { get; set; }

        public class InputModel
        {
            public string FullName { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Validate các trường input
            if (string.IsNullOrWhiteSpace(Input.FullName))
            {
                ErrorMessage = "Vui lòng nhập họ tên!";
                return Page();
            }

            if (!ValidationService.IsValidPhoneNumber(Input.Phone))
            {
                ErrorMessage = "Số điện thoại không hợp lệ! Số điện thoại phải có đúng 10 chữ số.";
                return Page();
            }

            if (!ValidationService.IsValidPassword(Input.Password))
            {
                ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự!";
                return Page();
            }

            // Kiểm tra xác nhận mật khẩu
            if (Input.Password != Input.ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu xác nhận không khớp!";
                return Page();
            }

            if (!string.IsNullOrWhiteSpace(Input.Email) && !ValidationService.IsValidEmail(Input.Email))
            {
                ErrorMessage = "Email không hợp lệ!";
                return Page();
            }

            // Kiểm tra số điện thoại đã tồn tại chưa
            var phoneExists = _context.Users.Any(u => u.Phone == Input.Phone);
            if (phoneExists)
            {
                ErrorMessage = "Số điện thoại đã được sử dụng!";
                return Page();
            }

            // Kiểm tra email nếu có
            if (!string.IsNullOrEmpty(Input.Email))
            {
                var emailExists = _context.Users.Any(u => u.Email == Input.Email);
                if (emailExists)
                {
                    ErrorMessage = "Email đã được sử dụng!";
                    return Page();
                }
            }

            var user = new OneTouch.Models.User
            {
                FullName = Input.FullName,
                Phone = Input.Phone,
                Email = Input.Email,
                PasswordHash = PasswordService.HashPassword(Input.Password),
                Role = "patient", // Mặc định là patient
                CreatedAt = System.DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Đăng ký xong chuyển sang trang đăng nhập
            return RedirectToPage("/Account/Login");
        }
    }
}