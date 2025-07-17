using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace OneTouch.Pages
{
    public class ContactModel : PageModel
    {
        [BindProperty]
        public string Name { get; set; }
        [BindProperty]
        public string Email { get; set; }
        [BindProperty]
        public string Message { get; set; }
        public string SuccessMessage { get; set; }

        public void OnGet() { }

        public void OnPost()
        {
            // Xử lý gửi liên hệ (có thể gửi email hoặc lưu DB ở đây)
            SuccessMessage = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất có thể.";
        }
    }
} 