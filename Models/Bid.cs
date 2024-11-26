using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models
{
    public class Bid
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        
        public Project Project { get; set; }
        public long FreelancerId { get; set; }
        public Freelancer Freelancer { get; set; }
        public decimal ProposedPrice { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
        public DateTime? ApprovedAt { get; set; }
    }
}
