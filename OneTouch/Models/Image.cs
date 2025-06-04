using System;

namespace OneTouch.Models
{
    public class Image
    {
        public int ImageId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Description { get; set; }
        public string Type { get; set; } // hero, feature, testimonial, etc.
        public DateTime CreatedAt { get; set; }
    }
} 