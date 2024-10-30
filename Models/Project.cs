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
        [Required]
        public string Title { get; set; }

        [AllowNull]
        public string Description { get; set; }

        // each project has one client
        public int ClientId { get; set; }//FK

        // Belongs to a client
        [ForeignKey("ClientId")]
        public Client Client { get; set; }


        // each project has one freelancer
        public int FreelancerId { get; set; }//FK
        // Belongs to a client
        [ForeignKey("FreelancerId")]
        public Freelancer Freelancer { get; set; }

        DateTime CreatedAt { get; set; }
    }
}
