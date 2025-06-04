using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OneTouch.Models;
using OneTouch.Services;
using Microsoft.EntityFrameworkCore;

namespace OneTouch.Pages;

public class IndexModel : PageModel
{
    private readonly ImageService _imageService;

    public IndexModel(ImageService imageService)
    {
        _imageService = imageService;
    }

    public Image HeroImage { get; set; }
    public List<Image> FeatureImages { get; set; }
    public List<Image> TestimonialImages { get; set; }
    public List<Doctor> Doctors { get; set; }
    public List<Image> DoctorImages { get; set; }

    public async Task OnGetAsync()
    {
        // Get hero image
        HeroImage = await _imageService.GetImageByTypeAsync("hero");

        // Get feature images
        FeatureImages = await _imageService.GetImagesByTypeAsync("feature");

        // Get testimonial images
        TestimonialImages = await _imageService.GetImagesByTypeAsync("testimonial");

        Doctors = await _imageService.GetDoctorsWithAvatarAsync();

        DoctorImages = await _imageService.GetImagesByTypeAsync("doctor");
    }
}
