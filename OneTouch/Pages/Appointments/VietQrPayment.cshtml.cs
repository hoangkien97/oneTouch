using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace OneTouch.Pages.Appointments
{
    public class VietQrPaymentModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string PaymentUrl { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string QrCodeUrl { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public double Amount { get; set; }

        [BindProperty(SupportsGet = true)]
        public string OrderDescription { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public int? ScheduleId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? UserId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Note { get; set; } = "";

        public bool ShowDebugInfo { get; set; } = true; // Hiển thị debug info

        public void OnGet()
        {
            // Log để debug
            System.Diagnostics.Debug.WriteLine($"[VietQrPayment] PaymentUrl: {PaymentUrl}");
            System.Diagnostics.Debug.WriteLine($"[VietQrPayment] QrCodeUrl: {QrCodeUrl}");
            System.Diagnostics.Debug.WriteLine($"[VietQrPayment] Amount: {Amount}");
            System.Diagnostics.Debug.WriteLine($"[VietQrPayment] OrderDescription: {OrderDescription}");
            System.Diagnostics.Debug.WriteLine($"[VietQrPayment] ScheduleId: {ScheduleId}");
            System.Diagnostics.Debug.WriteLine($"[VietQrPayment] UserId: {UserId}");
            System.Diagnostics.Debug.WriteLine($"[VietQrPayment] Note: {Note}");

            // Kiểm tra xem có dữ liệu không
            if (string.IsNullOrEmpty(PaymentUrl))
            {
                System.Diagnostics.Debug.WriteLine($"[VietQrPayment] WARNING: PaymentUrl is empty!");
            }
            if (string.IsNullOrEmpty(QrCodeUrl))
            {
                System.Diagnostics.Debug.WriteLine($"[VietQrPayment] WARNING: QrCodeUrl is empty!");
            }
        }
    }
} 