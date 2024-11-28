using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models
{
    public class ProjectLike
    {
        [Required]
        [Key]
        public long Id { get; set; }
 
        public long ProjectId { get; set; }
        public Project Project { get; set; }
    
        public long UserId { get; set; }
        public User user { get; set; }

        public DateTime? CreatedAt { get; set; }

    }
}
