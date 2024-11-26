using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class TaskUpdateDTO
    {
     
        [Required]
        [AllowedValues(["to-do", "in-progress", "in-review", "done"])]
        public string Status { get; set; }

        [MaxLength(512, ErrorMessage = "Name is too long.")]
        public string Name { get; set; }

        public DateTime deadlineAt { get; set; }

        [MaxLength(1024, ErrorMessage = "note is too long.")]
        public string? notes { get; set; }
    }
}
