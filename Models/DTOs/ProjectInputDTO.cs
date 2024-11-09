using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectInputDTO
    {
        [Required]
        [MaxLength(512, ErrorMessage ="Length too long")]
        public string Title { get; set; }

        [AllowNull]
        public string Description { get; set; }

        [Required]
        public string QualificationName { get; set; }

        [Required]
        public int Duration { get; set; }//Number of days

        [Required]
        [AllowedValues("PerHour","Fixed")]
        public string PriceType { get; set; }

        [Required]
        public decimal Budget { get; set; }


    }
}
