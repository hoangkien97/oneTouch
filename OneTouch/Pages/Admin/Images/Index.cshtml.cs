using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using OneTouch.Models;
using OneTouch.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneTouch.Pages.Admin.Images
{
    public class IndexModel : PageModel
    {
        private readonly ImageService _imageService;

        public IndexModel(ImageService imageService)
        {
            _imageService = imageService;
        }

        public List<Image> Images { get; set; }

        public async Task OnGetAsync()
        {
            // Lấy danh sách ảnh từ database
            Images = await _imageService.GetAllImagesAsync();
        }

        public async Task<IActionResult> OnPostAsync(IFormFile file, string type, string description)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn file ảnh");
                return Page();
            }

            try
            {
                await _imageService.UploadImageAsync(file, type, description);
                return RedirectToPage();
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _imageService.DeleteImageAsync(id);
            return RedirectToPage();
        }
    }
} 