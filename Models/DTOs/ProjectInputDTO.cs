using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs
{
    // خاص بادخال معلومات المشروع
    public class ProjectInputDTO
    {
        //public int Id { get; set; }

        [Required]
        [StringLength(250)]
        public string Title { get; set; }

        [AllowNull]
        public string Description { get; set; }

        [ForeignKey("ClientId")]
        public int ClientId { get; set; }

        [ForeignKey("FreelancerId")]
        public int FreelancerId { get; set; }
    }
}
