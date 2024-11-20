namespace AonFreelancing.Models.DTOs
{
    public class TaskOutputDTO
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string Name { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime? CompletedAt { get; set; }

        public TaskOutputDTO() { }
        public TaskOutputDTO(Models.Task task)
        {
            Id = task.Id;
            ProjectId = task.ProjectId;
            Name = task.Name;
            Notes = task.Notes;
            Status = task.Status;
            Deadline = task.Deadline;
            CompletedAt = task.CompletedAt;
        }

    }
}
