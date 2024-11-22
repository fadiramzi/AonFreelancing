using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class BidInputDTO
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Proposed price must be greater than zero.")]
        public decimal ProposedPrice { get; set; }

        [MaxLength(1024, ErrorMessage = "The notes message is too long !")]
        [Required]
        public string Notes { get; set; }
    }
}
