using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class BidInputDto
    {
        [Required(ErrorMessage = "Proposed price is required.")]
        [Range(0.01, 999999, ErrorMessage = "Proposed price must be greater than 0.")]
        public decimal ProposedPrice { get; set; }

        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
        public string? Notes { get; set; }
    }
}
