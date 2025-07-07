using Microsoft.AspNetCore.Mvc;
using OneTouch.Models;
using OneTouch.Services.Interfaces;

namespace OneTouch.Controllers
{
    [Route("[controller]")]
    public class TestController : Controller
    {
        private readonly IVietQrService _vietQrService;

        public TestController(IVietQrService vietQrService)
        {
            _vietQrService = vietQrService;
        }

        [HttpGet("vietqr")]
        public IActionResult TestVietQr()
        {
            var paymentInfo = new PaymentInformationModel
            {
                OrderType = "test",
                Amount = 200000,
                OrderDescription = "Test thanh toán VietQR",
                Name = "Test User"
            };

            var paymentUrl = _vietQrService.CreatePaymentUrl(paymentInfo, HttpContext);
            var qrCodeUrl = _vietQrService.GenerateQrCode(paymentUrl);

            ViewBag.PaymentUrl = paymentUrl;
            ViewBag.QrCodeUrl = qrCodeUrl;
            ViewBag.Amount = paymentInfo.Amount;
            ViewBag.Description = paymentInfo.OrderDescription;

            return View();
        }
    }
} 