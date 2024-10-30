using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models
{
    //Entity
    [Table("Projects")]
    public class Project
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [AllowNull]
        public string Description { get; set; }

        public int ClientId { get; set; }
        [ForeignKey("FreelancerId")]
        [AllowNull]
        public int? FreelancerId { get; set; }
        [ForeignKey("ClientId")]

        public Client Client { get; set; }
        [AllowNull]
        public Freelancer? Freelancer{ get; set; }

        DateTime CreatedAt { get; set; }
    }
}
