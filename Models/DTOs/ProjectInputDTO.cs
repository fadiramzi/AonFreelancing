using AonFreelancing.Attributes;
using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectInputDto
    {
        [Required]
        [MaxLength(512, ErrorMessage ="Title is too long.")]
        public string Title { get; set; }

        [MaxLength(1024,ErrorMessage = "Description is too long.")]
        public string? Description { get; set; }

        [Required]
        [AllowedValues(["uiux", "mobile", "frontend", "backend", "fullstack"])]
        public string QualificationName { get; set; }

        [Required]
        [Range(1, 365)]
        public int Duration { get; set; } //Number of days

        //[AllowedValues("PerHour","Fixed", ErrorMessage ="Price type is invalid.")]
        public string? PriceType { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public decimal Budget { get; set; }

        //allow only 5 MB of file size
        [MaxFileSize(1024 * 1024 * 5)]
        //allow only these extensions
        [AllowedFileExtensions([".jpg", ".jpeg", ".png"])]
        public IFormFile? ImageFile { get; set; }
    }
}
