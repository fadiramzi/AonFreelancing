using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class BidsOutDTO
    {
        public long Id { get; set; }
        public long FreelancerId { get; set; }
        public decimal ProposedPrice { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
    public class BidsInputDTO
    {
        [Required]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Invalid input.")]
        public decimal ProposedPrice { get; set; }

        [MaxLength(512, ErrorMessage = "Notes cannot exceed 512 characters.")]
        public string? Notes { get; set; }
    }
}
