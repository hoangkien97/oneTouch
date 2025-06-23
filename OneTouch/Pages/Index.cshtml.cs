using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OneTouch.Models;
using OneTouch.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OneTouch.Pages;

public class DoctorViewModel
{
    public int DoctorId { get; set; }
    public string FullName { get; set; }
    public string SpecialtyName { get; set; }
    public string AvatarPath { get; set; }
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public string Description { get; set; }
}

public class IndexModel : PageModel
{
    private readonly ImageService _imageService;
    private readonly OneTouchDbContext _context;

    public IndexModel(ImageService imageService, OneTouchDbContext context)
    {
        _imageService = imageService;
        _context = context;
    }

    public List<Image> HeroImages { get; set; }
    public List<Image> FeatureImages { get; set; }
    public List<Image> TestimonialImages { get; set; }
    public List<Doctor> Doctors { get; set; }
    public List<DoctorViewModel> TopDoctors { get; set; }
    public List<Image> DoctorImages { get; set; }
    public Image AboutImage { get; set; }

    public async Task OnGetAsync()
    {
        // Get hero images
        HeroImages = await _imageService.GetImagesByTypeAsync("hero");

        // Get feature images
        FeatureImages = await _imageService.GetImagesByTypeAsync("feature");

        // Get testimonial images
        TestimonialImages = await _imageService.GetImagesByTypeAsync("testimonial");

        Doctors = await _imageService.GetAllDoctorsWithUserAndSpecialtyAsync();

        DoctorImages = await _imageService.GetImagesByTypeAsync("doctor");

        var aboutImages = await _imageService.GetImagesByTypeAsync("about");
        AboutImage = aboutImages.OrderByDescending(i => i.CreatedAt).FirstOrDefault();

        // Get Top 3 Rated Doctors
        TopDoctors = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialty)
            .Include(d => d.Schedules)
                .ThenInclude(s => s.Appointments)
                    .ThenInclude(a => a.Feedbacks)
            .Where(d => d.User != null && d.Schedules.SelectMany(s => s.Appointments).SelectMany(a => a.Feedbacks).Any(f => f.Rating.HasValue))
            .Select(d => new DoctorViewModel
            {
                DoctorId = d.DoctorId,
                FullName = d.User.FullName,
                SpecialtyName = d.Specialty.Name,
                AvatarPath = d.AvatarPath,
                Description = d.Description,
                AverageRating = d.Schedules
                                    .SelectMany(s => s.Appointments)
                                    .SelectMany(a => a.Feedbacks)
                                    .Average(f => f.Rating.Value),
                RatingCount = d.Schedules
                                    .SelectMany(s => s.Appointments)
                                    .SelectMany(a => a.Feedbacks)
                                    .Count(f => f.Rating.HasValue)
            })
            .OrderByDescending(d => d.AverageRating)
            .ThenByDescending(d => d.RatingCount)
            .Take(3)
            .ToListAsync();
    }
}
