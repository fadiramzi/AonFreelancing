using AonFreelancing.Models.Responses;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models
{
    //Entity
    [Table("Projects")]
    public class Project
    {
        public int Id { get; set; }
        [Required] public string Title { get; set; }

        [AllowNull] public string Description { get; set; }

        public long ClientId { get; set; } //FK

        // Belongs to a client
        [ForeignKey("ClientId")] 
        public Client Client { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? StartDate { get; set; }
     
        public string ?ImageName { get; set; }
 
        public DateTime? EndDate { get; set; }
        public string PriceType { get; set; }
        public int Duration { get; set; }
        public decimal Budget { get; set; }
        public string QualificationName { get; set; }
        public string Status { get; set; } = "available";
        public long? FreelancerId { get; set; }
        [ForeignKey("FreelancerId")]
        public virtual Freelancer? Freelancer { get; set; }
        public List<Bid> ?Bids { get; set; }
        public List<EntityTask>?Tasks { get; set; }
    }
}
