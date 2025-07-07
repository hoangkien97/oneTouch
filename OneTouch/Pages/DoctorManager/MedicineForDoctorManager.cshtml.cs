using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OneTouch.Models;
using OneTouch.Pages.Dto;

namespace OneTouch.Pages.DoctorManager
{
    public class MedicineForDoctorManagerModel : PageModel
    {
        private readonly OneTouchDbContext _context;
        public MedicineForDoctorManagerModel(OneTouchDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; } // MedicalRecordId

        public int RecordId => Id;

        public List<PrescriptionDisplayItem> ExistingMedicines { get; set; } = new();

        [BindProperty]
        public CreateMedicineInMedicalRecordRequest NewMedicine { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadExistingMedicines();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Appointment)
                    .ThenInclude(a => a.Prescriptions)
                        .ThenInclude(p => p.PrescriptionDetails)
                            .ThenInclude(d => d.Medicine)
                .FirstOrDefaultAsync(m => m.RecordId == Id);

            if (medicalRecord == null)
                return NotFound("MedicalRecord not found");

            var appointment = medicalRecord.Appointment;
            if (appointment == null)
                return BadRequest("MedicalRecord has no Appointment.");

            // Kiểm tra Prescription có chưa
            if (appointment.Prescriptions.Any())
                return BadRequest("Đã tồn tại đơn thuốc. Không thể thêm mới.");

            // Tạo thuốc nếu chưa có
            var medicine = await _context.Medicines
                .FirstOrDefaultAsync(m => m.Name == NewMedicine.MedicineName);

            if (medicine == null)
            {
                medicine = new Medicine
                {
                    Name = NewMedicine.MedicineName,
                    Description = NewMedicine.Description,
                    Unit = NewMedicine.Unit,
                    Price = NewMedicine.Price
                };
                _context.Medicines.Add(medicine);
                await _context.SaveChangesAsync();
            }

            // Tạo đơn thuốc
            var prescription = new Prescription
            {
                AppointmentId = appointment.AppointmentId,
                CreatedAt = DateTime.Now,
                Note = null
            };
            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            // Chi tiết thuốc
            var detail = new PrescriptionDetail
            {
                PrescriptionId = prescription.PrescriptionId,
                MedicineId = medicine.MedicineId,
                Quantity = NewMedicine.Quantity,
                Dosage = NewMedicine.Dosage,
                Instructions = NewMedicine.Instructions
            };
            _context.PrescriptionDetails.Add(detail);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = Id });
        }

        private async Task LoadExistingMedicines()
        {
            ExistingMedicines = await _context.Prescriptions
                .Where(p => p.Appointment != null && p.Appointment.MedicalRecords.Any(mr => mr.RecordId == Id))
                .SelectMany(p => p.PrescriptionDetails)
                .Include(d => d.Medicine)
                .Select(d => new PrescriptionDisplayItem
                {
                    MedicineName = d.Medicine!.Name,
                    Unit = d.Medicine.Unit,
                    Quantity = d.Quantity ?? 0,
                    Dosage = d.Dosage,
                    Instructions = d.Instructions
                })
                .ToListAsync();
        }

    }
}
