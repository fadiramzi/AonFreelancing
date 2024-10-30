using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace AonFreelancing.Models
{
    //Entity
    [Table("Projects")]
    public class Project
    {

        //[JsonIgnore]  // ignore Id because Id created by default to the database (just for testing)
        public int Id { get; set; }
        [Required]
        public string? Title { get; set; }

        [AllowNull]
        public string Description { get; set; }

        public int ClientId { get; set; }//FK

        // Belongs to a client
        [ForeignKey("ClientId")]

        public Client? Client { get; set; }

        public int FreelancerId { get; set; }//FK

        [ForeignKey("FreelancerId")]

        public Freelancer? Freelancer { get; set; }

        DateTime CreatedAt { get; set; }



    }
}
