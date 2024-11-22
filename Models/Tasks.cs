using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models
{
    public class Tasks
    {
        [Key]
        public int Id { get; set; } 

        
        public int ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public Project Project { get; set; } 

        public string Name { get; set; }

        [AllowedValues("to-do", "in-progress", "in-review", "done")]
        public string Status { get; set; } = "To-Do"; // Default status

        public DateTime? DeadlineAt { get; set; } // Deadline for the task
        public DateTime? CompletedAt { get; set; } // Date when the task was completed

        [MaxLength(1024)]
        public string? Notes { get; set; } // Optional notes about the task
    }
}
