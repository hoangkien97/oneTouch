using System;

namespace OneTouch.Models
{
    public class Expert
    {
        public int ExpertId { get; set; }
        public string FullName { get; set; }
        public string Title { get; set; }
        public string ImagePath { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 