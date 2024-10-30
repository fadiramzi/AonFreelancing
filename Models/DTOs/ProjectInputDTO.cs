using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectInputDTO
    {
        [Required]
        //[MaxLength(80, ErrorMessage = "Try shorter name .")]
        public string Title { get; set; }

        [AllowNull]
        public string Description { get; set; }


        // ensures select a Client and Freelancer for the entered project
        [Required]
        public int ClientId { get; set; }//FK
        [Required]
        public int FreelancerId { get; set; }//FK
    }
}
