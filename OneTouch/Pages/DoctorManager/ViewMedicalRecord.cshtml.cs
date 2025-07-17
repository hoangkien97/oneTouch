using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OneTouch.Models;
using OneTouch.Pages.Dto;
using Microsoft.EntityFrameworkCore;


namespace OneTouch.Pages.DoctorManager
{
    public class ViewMedicalRecordModel : PageModel
    {
        private readonly OneTouchDbContext _context;

        public ViewMedicalRecordModel(OneTouchDbContext context)
        {
            _context = context;
        }

        public List<MedicalRecordItemResponse> Records { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int DoctorId { get; set; }

        public async Task OnGetAsync()
        {
            // Lấy danh sách MedicalRecord của bác sĩ này
            Records = await _context.MedicalRecords
                .Include(mr => mr.Appointment)
                    .ThenInclude(a => a.User)
                .Where(mr => mr.Appointment != null
                             && mr.Appointment.Schedule != null
                             && mr.Appointment.Schedule.DoctorId == DoctorId)
                .Select(mr => new MedicalRecordItemResponse
                {
                    RecordId = mr.RecordId,
                    PatientName = mr.Appointment.User!.FullName ?? "Unknown",
                    CreatedAt = mr.CreatedAt,
                    Diagnosis = mr.Diagnosis
                })
                .ToListAsync();
        }
    }
}
