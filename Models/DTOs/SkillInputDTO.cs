using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class SkillInputDTO
    {
        [Required(ErrorMessage = "skill name cannot be null")]
        public string Name { get; set; }
    }
}
