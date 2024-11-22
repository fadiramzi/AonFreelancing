using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models
{
    public class Bid
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        
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
