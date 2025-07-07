using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OneTouch.Models;
using OneTouch.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Net;

namespace OneTouch.Services
{
    public class VietQrService : IVietQrService
    {
        private readonly IConfiguration _configuration;

        public VietQrService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            var bankCode = _configuration["VietQr:BankCode"];
            var accountNo = _configuration["VietQr:AccountNo"];
            var accountName = _configuration["VietQr:AccountName"];
            var amount = ((int)model.Amount).ToString();
            var content = Uri.EscapeDataString(model.OrderDescription ?? "Thanh toan dat lich kham");

            // Trả về trực tiếp URL ảnh QR code VietQR
            return $"https://img.vietqr.io/image/{bankCode}-{accountNo}-compact2.png?amount={amount}&addInfo={content}&accountName={Uri.EscapeDataString(accountName)}";
        }

        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var response = new PaymentResponseModel
            {
                Success = true,
                PaymentMethod = "VietQR",
                OrderDescription = collections["content"].FirstOrDefault() ?? "",
                OrderId = collections["orderId"].FirstOrDefault() ?? DateTime.Now.Ticks.ToString(),
                PaymentId = collections["transactionId"].FirstOrDefault() ?? DateTime.Now.Ticks.ToString(),
                TransactionId = collections["transactionId"].FirstOrDefault() ?? DateTime.Now.Ticks.ToString(),
                Token = collections["signature"].FirstOrDefault() ?? ""
            };
            return response;
        }

        public string GenerateQrCode(string paymentUrl)
        {
            // Không cần dùng Google Charts nữa, trả về luôn paymentUrl
            return paymentUrl;
        }
    }
} 