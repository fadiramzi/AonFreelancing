using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models
{
    public class Skill
    {

        [Required]
        public long Id { get; set; }

        public long UserId { get; set; }
        public Freelancer freelancer { get; set; }

        public string Name { get; set; }
    }
}
