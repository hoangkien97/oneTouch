using Microsoft.AspNetCore.Http;
using OneTouch.Models;

namespace OneTouch.Services.Interfaces
{
    public interface IVietQrService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);
        string GenerateQrCode(string paymentUrl);
    }
} 