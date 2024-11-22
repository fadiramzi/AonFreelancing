using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models.Responses
{
    public class Bid
    {
        [Key]
        public int id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [ForeignKey("ProjectId")]

        public Project Project { get; set; }

        public long FreelancerId { get; set; }
        [ForeignKey("FreelancerId")]
       public Freelancer Freelancer { get; set; }
        public decimal proposed_Price { get; set; }
        public string Nots {  get; set; }

        [AllowedValues("approved","Pending")]
        public string status { get; set; } = "pending";
        public DateTime submittedAt { get; set; }

        public DateTime ApprovedAt{ get; set; }
    }
}
