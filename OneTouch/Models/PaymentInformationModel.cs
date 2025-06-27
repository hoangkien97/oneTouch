using System;
using System.ComponentModel.DataAnnotations;

namespace OneTouch.Models
{
    public class PaymentInformationModel
    {
        public string OrderType { get; set; }
        public double Amount { get; set; }
        public string OrderDescription { get; set; }
        public string Name { get; set; }
        
        // Thêm thông tin appointment
        public int? ScheduleId { get; set; }
        public int? UserId { get; set; }
        public string Note { get; set; }
    }
} 