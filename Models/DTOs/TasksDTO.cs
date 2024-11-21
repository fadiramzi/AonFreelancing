using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class TasksOutDTO
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string Name { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class TasksInputDTO
    {
        [Required ]
        [MinLength(3, ErrorMessage = "Task name is too short.")]
        public string Name { get; set; }
        [MaxLength(512, ErrorMessage = "Notes cannot exceed 512 characters.")]
        public string? Notes { get; set; }
        [Required]
        public DateTime DeadlineAt { get; set; }
    }
}
