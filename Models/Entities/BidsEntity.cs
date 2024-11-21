using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models.Entities
{
    [Table("Bids")]
    public class BidsEntity
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public ProjectEntity Project { get; set; }
        public long FreelancerId { get; set; }
        public FreelancerEntity Freelancer { get; set; }
        public decimal ProposedPrice { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
