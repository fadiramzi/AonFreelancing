using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class TaskInputDTO
    {
        
        [MaxLength(100)]
        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime DeadlineAt { get; set; }

        [MaxLength(255)]
        public string? Notes { get; set; }

    }
}
