using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models
{
    //Entity
    [Table("Projects")]
    public class Project
    {
        public long Id { get; set; }
        [Required] public string Title { get; set; }

        [AllowNull] public string Description { get; set; }

        public long ClientId { get; set; } //FK

        // Belongs to a client
        [ForeignKey("ClientId")] 
        public Client Client { get; set; }

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
        public virtual Freelancer? Freelancer { get; set; }
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public string? ImagePath { get; set; }

        public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();

    }
}
