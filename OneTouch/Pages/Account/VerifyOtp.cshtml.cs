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

            // Nếu là xác thực đăng ký
            if (Request.Query.ContainsKey("register") && TempData["RegisterOtp"] != null)
            {
                var otp = TempData["RegisterOtp"]?.ToString();
                var phone = TempData["RegisterPhone"]?.ToString();
                var fullName = TempData["RegisterFullName"]?.ToString();
                var email = TempData["RegisterEmail"]?.ToString();
                var passwordHash = TempData["RegisterPasswordHash"]?.ToString();

                // Kiểm tra đủ thông tin
                if (string.IsNullOrEmpty(otp) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(passwordHash))
                {
                    ErrorMessage = "Thiếu thông tin đăng ký. Vui lòng đăng ký lại.";
                    return Page();
                }

                if (Input.Otp != otp || Input.Phone != phone)
                {
                    ErrorMessage = "Mã OTP không đúng!";
                    return Page();
                }

                // Kiểm tra lại số điện thoại chưa tồn tại
                if (_context.Users.Any(u => u.Phone == phone))
                {
                    ErrorMessage = "Số điện thoại đã được sử dụng!";
                    return Page();
                }

                // Tạo user mới
                var newUser = new OneTouch.Models.User
                {
                    FullName = fullName,
                    Phone = phone,
                    Email = string.IsNullOrEmpty(email) ? null : email,
                    PasswordHash = passwordHash,
                    Role = "patient",
                    CreatedAt = DateTime.Now
                };
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Xóa TempData liên quan đăng ký
                TempData.Remove("RegisterOtp");
                TempData.Remove("RegisterPhone");
                TempData.Remove("RegisterFullName");
                TempData.Remove("RegisterEmail");
                TempData.Remove("RegisterPasswordHash");

                SuccessMessage = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToPage("/Account/Login");
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