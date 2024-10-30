using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectInputDTO
    {
        [Required]
        [StringLength(128)]
        public string Title { get; set; }

        [AllowNull]
        public string Description { get; set; }

        [Required]
        public int ClientId { get; set; }//FK
        
        public int? FreelancerId { get; set; }
    }
    
    public class ProjectOutDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public int ClientId { get; set; }//FK
        
        public int? FreelancerId { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}
