using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

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
        [Range(30,730)]
        public int Duration { get; set; } //Number of days

        //[Required]
        //[AllowedValues("PerHour", "Fixed", ErrorMessage = "Price type is invalid.")]
        //public string PriceType { get; set; }

        [Required]
        [Range(0, 1000)]
        public decimal Budget { get; set; }
    }
}
