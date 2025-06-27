using Microsoft.AspNetCore.Mvc;
using OneTouch.Models;
using OneTouch.Services.Interfaces;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneTouch.Controllers
{
    [Route("[controller]")]
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly OneTouch.Models.OneTouchDbContext _context;
        private readonly IEmailService _emailService;
        
        public PaymentController(IVnPayService vnPayService, OneTouch.Models.OneTouchDbContext context, IEmailService emailService)
        {
            _vnPayService = vnPayService;
            _context = context;
            _emailService = emailService;
        }

        [HttpGet("CreatePaymentUrlVnpay")]
        public IActionResult CreatePaymentUrlVnpayGet([FromQuery] PaymentInformationModel model)
        {
            // Log tất cả query parameters để debug
            System.Diagnostics.Debug.WriteLine($"[PaymentController] Query parameters received:");
            foreach (var key in Request.Query.Keys)
            {
                System.Diagnostics.Debug.WriteLine($"[PaymentController] {key}: {Request.Query[key]}");
            }
            
            // Log để debug
            System.Diagnostics.Debug.WriteLine($"[PaymentController] Received model - Amount: {model.Amount}, OrderDescription: {model.OrderDescription}");
            
            // Thử parse amount trực tiếp từ query string nếu model binding không hoạt động
            if (model.Amount <= 0 && Request.Query.ContainsKey("Amount"))
            {
                if (double.TryParse(Request.Query["Amount"], out var parsedAmount))
                {
                    model.Amount = parsedAmount;
                    System.Diagnostics.Debug.WriteLine($"[PaymentController] Parsed Amount from query string: {model.Amount}");
                }
            }
            
            // Validation và fallback
            if (model.Amount <= 0)
            {
                System.Diagnostics.Debug.WriteLine($"[PaymentController] ERROR: Amount is {model.Amount}, setting to default 200000");
                model.Amount = 200000; // Fallback value
            }
            
            // Đảm bảo các trường bắt buộc có giá trị
            if (string.IsNullOrEmpty(model.OrderType))
                model.OrderType = "appointment";
            if (string.IsNullOrEmpty(model.Name))
                model.Name = "Khách hàng";
            if (string.IsNullOrEmpty(model.OrderDescription))
                model.OrderDescription = "Thanh toán đặt lịch khám";
            
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Redirect(url);
        }

        [HttpPost("CreatePaymentUrlVnpay")]
        public IActionResult CreatePaymentUrlVnpay([FromForm] PaymentInformationModel model)
        {
            // Log tất cả form data để debug
            System.Diagnostics.Debug.WriteLine($"[PaymentController] Form data received:");
            foreach (var key in Request.Form.Keys)
            {
                System.Diagnostics.Debug.WriteLine($"[PaymentController] {key}: {Request.Form[key]}");
            }
            
            // Log để debug
            System.Diagnostics.Debug.WriteLine($"[PaymentController] Received model - Amount: {model.Amount}, OrderDescription: {model.OrderDescription}");
            
            // Validation và fallback
            if (model.Amount <= 0)
            {
                System.Diagnostics.Debug.WriteLine($"[PaymentController] ERROR: Amount is {model.Amount}, setting to default 200000");
                model.Amount = 200000; // Fallback value
            }
            
            // Đảm bảo các trường bắt buộc có giá trị
            if (string.IsNullOrEmpty(model.OrderType))
                model.OrderType = "appointment";
            if (string.IsNullOrEmpty(model.Name))
                model.Name = "Khách hàng";
            if (string.IsNullOrEmpty(model.OrderDescription))
                model.OrderDescription = "Thanh toán đặt lịch khám";
            
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Redirect(url);
        }

        [HttpGet("PaymentCallbackVnpay")]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            try
            {
                // Log tất cả query parameters từ VNPay
                System.Diagnostics.Debug.WriteLine($"[PaymentCallback] VNPay callback received:");
                foreach (var key in Request.Query.Keys)
                {
                    System.Diagnostics.Debug.WriteLine($"[PaymentCallback] {key}: {Request.Query[key]}");
                }
                
                // Xử lý callback VNPay
                var response = _vnPayService.PaymentExecute(Request.Query);
                
                System.Diagnostics.Debug.WriteLine($"[PaymentCallback] Response - Success: {response.Success}, ResponseCode: {response.VnPayResponseCode}");
            
                // Kiểm tra kết quả
                if (response.Success)
                {
                    // Xử lý thông tin đặt lịch từ OrderInfo
                    var orderInfo = Request.Query["vnp_OrderInfo"].FirstOrDefault() ?? "";
                    System.Diagnostics.Debug.WriteLine($"[PaymentCallback] Raw OrderInfo: {orderInfo}");
                    
                    if (!string.IsNullOrEmpty(orderInfo))
                    {
                        try
                        {
                            var decodedOrderInfo = Uri.UnescapeDataString(orderInfo);
                            System.Diagnostics.Debug.WriteLine($"[PaymentCallback] Decoded OrderInfo: {decodedOrderInfo}");
                            
                            // Parse theo format mới: "Khách hàng Thanh toán đặt lịch khám 200000|ScheduleId|UserId|Amount|Note"
                            if (decodedOrderInfo.Contains("|"))
                            {
                                var parts = decodedOrderInfo.Split('|');
                                System.Diagnostics.Debug.WriteLine($"[PaymentCallback] Parts count: {parts.Length}");
                                System.Diagnostics.Debug.WriteLine($"[PaymentCallback] Parts: {string.Join(", ", parts)}");
                                
                                // Kiểm tra xem có đủ thông tin appointment không (ít nhất 5 phần)
                                if (parts.Length >= 5)
                                {
                                    // Phần cuối chứa thông tin appointment: ScheduleId|UserId|Amount|Note
                                    var appointmentParts = parts.Skip(parts.Length - 4).ToArray();
                                    
                                    if (int.TryParse(appointmentParts[0], out int scheduleId) && 
                                        int.TryParse(appointmentParts[1], out int userId) && 
                                        decimal.TryParse(appointmentParts[2], out decimal paidAmount))
                                    {
                                        string note = appointmentParts[3];

                                        System.Diagnostics.Debug.WriteLine($"[PaymentCallback] Successfully parsed - ScheduleId: {scheduleId}, UserId: {userId}, Amount: {paidAmount}, Note: {note}");

                                        // Kiểm tra xem appointment đã tồn tại chưa
                                        var existingAppointment = _context.Appointments
                                            .FirstOrDefault(a => a.ScheduleId == scheduleId && a.UserId == userId);
                                        
                                        if (existingAppointment == null)
                                        {
                                            // Tạo appointment
                                            var appointment = new Appointment
                            {
                                ScheduleId = scheduleId,
                                UserId = userId,
                                                Status = "Confirmed",
                                Note = note,
                                CreatedAt = DateTime.Now
                            };
                            _context.Appointments.Add(appointment);
                            _context.SaveChanges();
                            
                                            System.Diagnostics.Debug.WriteLine($"[PaymentCallback] SUCCESS: Created appointment with ID: {appointment.AppointmentId}");

                                            // Tạo invoice
                            var invoice = new Invoice
                            {
                                AppointmentId = appointment.AppointmentId,
                                                TotalAmount = paidAmount,
                                PaymentStatus = "Paid",
                                PaymentMethod = "VnPay",
                                CreatedAt = DateTime.Now
                            };
                            _context.Invoices.Add(invoice);
                            _context.SaveChanges();
                            
                                            System.Diagnostics.Debug.WriteLine($"[PaymentCallback] SUCCESS: Created invoice with ID: {invoice.InvoiceId}");

                                            // Gửi email xác nhận đặt lịch
                                            try
                                            {
                                                await _emailService.SendAppointmentConfirmationEmailAsync(appointment, invoice, response.TransactionId);
                                                System.Diagnostics.Debug.WriteLine($"[PaymentCallback] SUCCESS: Sent confirmation email for appointment ID: {appointment.AppointmentId}");
                                            }
                                            catch (Exception emailEx)
                                            {
                                                System.Diagnostics.Debug.WriteLine($"[PaymentCallback] WARNING: Failed to send email: {emailEx.Message}");
                                                // Không throw exception vì email không quan trọng bằng việc tạo appointment
                                            }
                                        }
                                        else
                                        {
                                            System.Diagnostics.Debug.WriteLine($"[PaymentCallback] Appointment already exists with ID: {existingAppointment.AppointmentId}");
                                        }
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine($"[PaymentCallback] FAILED: Could not parse appointment data parts");
                                    }
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"[PaymentCallback] FAILED: Not enough parts in OrderInfo. Expected at least 5, got {parts.Length}");
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[PaymentCallback] FAILED: OrderInfo does not contain '|' character");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[PaymentCallback] ERROR processing OrderInfo: {ex.Message}");
                            System.Diagnostics.Debug.WriteLine($"[PaymentCallback] Stack trace: {ex.StackTrace}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[PaymentCallback] OrderInfo is null or empty");
                    }

                    // Redirect về trang thành công
                    return RedirectToPage("/Appointments/PaymentSuccess", new { 
                        transactionId = response.TransactionId, 
                        payDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") 
                    });
                }
                else
                {
                    // Redirect về trang thất bại
                    var errorMessage = $"Giao dịch thất bại! Mã lỗi: {response.VnPayResponseCode}";
                        
                    return RedirectToPage("/Appointments/PaymentSuccess", new { 
                        error = errorMessage 
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PaymentCallback] ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[PaymentCallback] Stack trace: {ex.StackTrace}");
                return RedirectToPage("/Appointments/PaymentSuccess", new { 
                    error = "Đã xảy ra lỗi khi xử lý callback." 
                });
            }
        }
    }
} 