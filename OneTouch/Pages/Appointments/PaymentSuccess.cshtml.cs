using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace OneTouch.Pages.Appointments
{
    public class PaymentSuccessModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string TransactionId { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string PayDate { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string Error { get; set; } = "";

        public bool IsSuccess => !string.IsNullOrEmpty(TransactionId) && string.IsNullOrEmpty(Error);

        public void OnGet()
        {
            // Thêm cache control headers để ngăn browser cache
            Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Add("Pragma", "no-cache");
            Response.Headers.Add("Expires", "0");

            // Log để debug
            System.Diagnostics.Debug.WriteLine($"[PaymentSuccess] TransactionId: {TransactionId}");
            System.Diagnostics.Debug.WriteLine($"[PaymentSuccess] PayDate: {PayDate}");
            System.Diagnostics.Debug.WriteLine($"[PaymentSuccess] Error: {Error}");
            System.Diagnostics.Debug.WriteLine($"[PaymentSuccess] IsSuccess: {IsSuccess}");
        }
    }
} 