using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.Entities
{
    //Entity
    [Table("Projects")]
    public class ProjectEntity
    {
        public long Id { get; set; }
        [Required] public string Title { get; set; }

        [AllowNull] public string Description { get; set; }

        public long ClientId { get; set; } //FK

        // Belongs to a client
        [ForeignKey("ClientId")]
        public ClientEntity Client { get; set; }
        public string? ImageName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string PriceType { get; set; }
        public int Duration { get; set; }
        public decimal Budget { get; set; }
        public string QualificationName { get; set; }
        public string Status { get; set; }
        public long? FreelancerId { get; set; }
        [ForeignKey("FreelancerId")]
        public FreelancerEntity? Freelancer { get; set; }
        public List<BidsEntity> Bids { get; set; }
    }
}
