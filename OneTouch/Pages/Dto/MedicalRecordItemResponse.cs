namespace OneTouch.Pages.Dto
{
    public class MedicalRecordItemResponse
    {
        public int RecordId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public string? Diagnosis { get; set; }
    }
}
