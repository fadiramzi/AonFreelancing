using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models
{
    public class Bid
    {

        [Key]
        public long Id { get; set; }

        public int ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public Project Project { get; set; } // FK

        public long FreelancerId { get; set; }
        [ForeignKey("FreelancerId")]
        public Freelancer Freelancer { get; set; } // FK

        public decimal ProposedPrice { get; set; }
        public string? Notes { get; set; }

        [AllowedValues("pending", "approved")]
        public string Status { get; set; } = "pending"; // Default status
        public DateTime SubmittedAt { get; set; } 
        public DateTime? ApprovedAt { get; set; }
    }
}
