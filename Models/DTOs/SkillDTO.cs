using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class SkillDTO
    {

        [Required]
        public string Name { get; set; }
    }
    public class SkillDeleteDTO
    {

        [Required]
        public string Name { get; set; }
        [Required]
        public long UserId { get; set; }
    }
}
