using System;
using System.Text.RegularExpressions;

namespace OneTouch.Services
{
    public class ValidationService
    {
        public static bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Kiểm tra số điện thoại phải đúng 10 số và chỉ chứa số
            return Regex.IsMatch(phone, @"^[0-9]{10}$");
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true; // Email là tùy chọn nên trả về true nếu null hoặc empty

            try
            {
                // Kiểm tra email có đúng định dạng
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // Mật khẩu phải có ít nhất 6 ký tự
            return password.Length >= 6;
        }
    }
} 