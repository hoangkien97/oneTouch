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

        public bool IsSuccess => string.IsNullOrEmpty(Error);

        public void OnGet()
        {
            // Log để debug
            System.Diagnostics.Debug.WriteLine($"[PaymentSuccess] TransactionId: {TransactionId}");
            System.Diagnostics.Debug.WriteLine($"[PaymentSuccess] PayDate: {PayDate}");
            System.Diagnostics.Debug.WriteLine($"[PaymentSuccess] Error: {Error}");
        }
    }
} 