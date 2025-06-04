using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using OneTouch.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace OneTouch.Services
{
    public class ImageService
    {
        private readonly OneTouchDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        public ImageService(OneTouchDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<Image> UploadImageAsync(IFormFile file, string type, string description)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded");

            // Kiểm tra định dạng file
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!Array.Exists(AllowedExtensions, x => x == extension))
                throw new ArgumentException("Invalid file type. Allowed types: " + string.Join(", ", AllowedExtensions));

            // Tạo tên file duy nhất
            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", type);
            
            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);

            // Lưu file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Lưu thông tin vào database
            var image = new Image
            {
                FileName = fileName,
                FilePath = $"/uploads/{type}/{fileName}",
                Description = description,
                Type = type,
                CreatedAt = DateTime.Now
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            return image;
        }

        public async Task DeleteImageAsync(int imageId)
        {
            var image = await _context.Images.FindAsync(imageId);
            if (image == null)
                return;

            // Xóa file
            var filePath = Path.Combine(_environment.WebRootPath, image.FilePath.TrimStart('/'));
            if (File.Exists(filePath))
                File.Delete(filePath);

            // Xóa record trong database
            _context.Images.Remove(image);
            await _context.SaveChangesAsync();
        }

        public async Task<Image> GetImageByIdAsync(int imageId)
        {
            return await _context.Images.FindAsync(imageId);
        }

        public async Task<Image> GetImageByTypeAsync(string type)
        {
            return await _context.Images.FirstOrDefaultAsync(i => i.Type == type);
        }

        public async Task<List<Image>> GetImagesByTypeAsync(string type)
        {
            return await _context.Images
                .Where(i => i.Type == type)
                .ToListAsync();
        }

        public async Task<List<Image>> GetAllImagesAsync()
        {
            return await _context.Images
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Doctor>> GetDoctorsWithAvatarAsync()
        {
            return await _context.Doctors
                .Where(d => d.AvatarPath != null && d.AvatarPath != "")
                .Include(d => d.User)
                .ToListAsync();
        }
    }
} 