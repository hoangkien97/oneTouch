using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OneTouch.Models;
using OneTouch.Services.Interfaces;

namespace OneTouch.Pages.Appointments
{
    public class TestVietQrModel : PageModel
    {
        private readonly IVietQrService _vietQrService;

        public TestVietQrModel(IVietQrService vietQrService)
        {
            _vietQrService = vietQrService;
        }

        [BindProperty]
        public double Amount { get; set; } = 200000;

        [BindProperty]
        public string Description { get; set; } = "Test thanh toán VietQR";

        public string QrCodeUrl { get; set; } = "";
        public string PaymentUrl { get; set; } = "";

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var paymentInfo = new PaymentInformationModel
            {
                OrderType = "test",
                Amount = Amount,
                OrderDescription = Description,
                Name = "Test User"
            };

            PaymentUrl = _vietQrService.CreatePaymentUrl(paymentInfo, HttpContext);
            QrCodeUrl = _vietQrService.GenerateQrCode(PaymentUrl);

            return Page();
        }
    }
} 