using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectInputDTO
    {
        [Required]
        [MaxLength(512, ErrorMessage = "the Title should be less than 512 char")]
        public string Title { get; set; }
        [AllowNull]
        [MaxLength(1024, ErrorMessage = "the Description should be less than 1024 char")]
        public string Description { get; set; }

        [Required]
        [AllowedValues("UI/UX", "FrontEnd", "BackEnd", "Mobile Application", "FullStack")]
        public string QualificationName { get; set; }
        [Required]
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


    }
}
