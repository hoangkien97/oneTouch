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

            // Sinh mã code thanh toán theo cấu trúc DH + số nguyên (3-10 ký tự)
            string paymentCode = model.Note;
            if (string.IsNullOrEmpty(paymentCode) || !paymentCode.StartsWith("DH"))
            {
                var number = (DateTime.UtcNow.Ticks % 1000000000).ToString(); // 9 ký tự
                paymentCode = $"DH{number}";
            }

            // Nội dung chuyển khoản chỉ là mã code
            var content = Uri.EscapeDataString(paymentCode);

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
           
            return paymentUrl;
        }
    }
} 