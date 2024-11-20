using AonFreelancing.Models.DTOs;
using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models
{
    [Table("Tasks")]
    public class Task
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string Name { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime? CompletedAt { get; set; }

        public Task() { }
        public Task(TaskInputDTO taskInputDTO,long projectId)
        {
            ProjectId = projectId;
            Name = taskInputDTO.Name;
            Notes = taskInputDTO.Notes;
            Deadline = taskInputDTO.DeadlineAt;
        }
    }
}
