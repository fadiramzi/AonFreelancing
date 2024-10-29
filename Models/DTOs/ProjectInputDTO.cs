using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectInputDTO
    {
        [Required]
        public string Title { get; set; }

        [AllowNull]
        public string? Description { get; set; }
        
        [Required]
        public int? ClientId { get; set; }//FK
    }
}
