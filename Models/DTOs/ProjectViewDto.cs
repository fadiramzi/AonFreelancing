using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs
{
    // خاص بعرض معلومات المشروع
    public class ProjectViewDto
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }

        [AllowNull]
        public string Description { get; set; }

        public int ClientId { get; set; } 

        public int FreelancerId { get; set; }      

        DateTime CreatedAt { get; set; }
    }
}
