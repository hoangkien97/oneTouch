using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OneTouch.Services.Interfaces;
using Vonage;
using Vonage.Request;

namespace OneTouch.Services
{
    public class VonageSmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly VonageClient _vonageClient;
        private const string TEST_PHONE_NUMBER = "+84333181665"; 

        public VonageSmsService(IConfiguration configuration)
        {
            _configuration = configuration;
            var credentials = Credentials.FromApiKeyAndSecret(
                _configuration["Vonage:ApiKey"],
                _configuration["Vonage:ApiSecret"]
            );
            _vonageClient = new VonageClient(credentials);
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                // Thêm mã quốc gia nếu chưa có
                if (!phoneNumber.StartsWith("+"))
                {
                    phoneNumber = "+84" + phoneNumber.TrimStart('0');
                }

                var response = await _vonageClient.SmsClient.SendAnSmsAsync(new Vonage.Messaging.SendSmsRequest
                {
                    To = phoneNumber,
                    From = TEST_PHONE_NUMBER,
                    Text = message
                });

                return response.Messages[0].Status == "0";
            }
            catch (Exception ex)
            {
                // Log lỗi để debug
                Console.WriteLine($"Error sending SMS: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendOtpAsync(string phoneNumber, string otp)
        {
            var message = $"Your OneTouch password reset OTP is: {otp}. This OTP will expire in 5 minutes.";
            return await SendSmsAsync(phoneNumber, message);
        }
    }
} 