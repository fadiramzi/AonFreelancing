using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectInputDto
    {
        [Required]
        [MaxLength(512, ErrorMessage = "Length is too long.")]
        public string Title { get; set; }
        [AllowNull]
        public string Description { get; set; }
        [Required]
        public string Qualification { get; set; }
        [Required]
        public int Duration { get; set; } // Number of days
        [Required]
        [AllowedValues("PerHour", "Fixed")]
        public string PriceType { get; set; }
        [Required]
        public decimal Budget { get; set; }
    }
}
