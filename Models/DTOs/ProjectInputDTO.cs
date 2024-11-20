using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectInputDto
    {
        [Required]
<<<<<<< HEAD
        [MaxLength(512, ErrorMessage = "the Title should be less than 512 char")]
        public string Title { get; set; }
        [AllowNull]
        [MaxLength(1024, ErrorMessage = "the Description should be less than 1024 char")]
        public string Description { get; set; }

        [Required]
        [AllowedValues("UI/UX", "FrontEnd", "BackEnd", "Mobile Application", "FullStack")]
=======
        [MaxLength(512, ErrorMessage ="Title is too long.")]
        public string Title { get; set; }

        [MaxLength(1024,ErrorMessage = "Description is too long.")]
        public string? Description { get; set; }

        [Required]
        [AllowedValues(["uiux", "mobile", "frontend", "backend", "fullstack"])]
>>>>>>> bd49e789eca82cf0b70e0aad4d121920d1c2c3b2
        public string QualificationName { get; set; }
        [Required]
<<<<<<< HEAD
        public DateTime CreatedAt { get; set; }
        [Required]
        public int Duration { get; set; }
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "specify your budget correctly")]
        public decimal Budget { get; set; }
        [Required]
        [AllowedValues("PerHour", "Fixed")]
        public string PriceType { get; set; }
        [Required]
        [AllowedValues("Available", "Closed")]
        public string Status { get; set; }
        [Required]
        public long ClientId { get; set; }//FK


=======
        [Range(1,int.MaxValue)]
        public int Duration { get; set; } //Number of days

        [Required]
        [AllowedValues("PerHour","Fixed", ErrorMessage ="Price type is invalid.")]
        public string PriceType { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public decimal Budget { get; set; }
>>>>>>> bd49e789eca82cf0b70e0aad4d121920d1c2c3b2
    }
}
