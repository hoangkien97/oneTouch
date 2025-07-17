namespace OneTouch.Pages.Dto
{
    public class CreateMedicineInMedicalRecordRequest
    {
        public int MedicalRecordId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Unit { get; set; }
        public decimal? Price { get; set; }
        public int Quantity { get; set; }
        public string? Dosage { get; set; }
        public string? Instructions { get; set; }
    }
}
