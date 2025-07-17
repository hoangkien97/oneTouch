using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OneTouch.Models;
using OneTouch.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace OneTouch.Pages
{
    public class ProfileModel : PageModel
    {
        private readonly IUserService _userService;

        public ProfileModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
            [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập email")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
            [StringLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự")]
            [RegularExpression(@"^[0-9]+$", ErrorMessage = "Số điện thoại chỉ được chứa các chữ số")]
            public string Phone { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToPage("/Account/Login");
            }

            if (!int.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Account/Login");
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            Input = new InputModel
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToPage("/Account/Login");
            }

            if (!int.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Account/Login");
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Kiểm tra email đã tồn tại chưa (nếu email thay đổi)
            if (user.Email != Input.Email)
            {
                var existingUser = await _userService.GetUserByEmailAsync(Input.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Input.Email", "Email này đã được sử dụng bởi tài khoản khác");
                    return Page();
                }
            }

            // Cập nhật thông tin
            user.FullName = Input.FullName;
            user.Email = Input.Email;
            user.Phone = Input.Phone;

            await _userService.UpdateUserAsync(user);

            // Cập nhật session
            HttpContext.Session.SetString("UserName", user.FullName);

            SuccessMessage = "Cập nhật thông tin thành công!";
            return Page();
        }
    }
} 