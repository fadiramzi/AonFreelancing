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

        public int ClientId { get; set; }//FK

        // Belongs to a client
        [ForeignKey("ClientId")]
        public Client Client { get; set; }


        public int FreeLancerId { get; set; }//FK

        // Belongs to a client
        [ForeignKey("FreeLancerId")]
        public Freelancer Freelancer { get; set; }

        DateTime CreatedAt { get; set; }



    }
}
