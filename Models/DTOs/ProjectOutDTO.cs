using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectOutDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public int ClientId { get; set; }//FK
        [ForeignKey("ClientId")]

        public int FreelancerId { get; set; }//FK
        [ForeignKey("FreelancerId")]


        DateTime CreatedAt { get; set; }
    }
}
