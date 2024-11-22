namespace AonFreelancing.Models
{
    public class TaskEntity
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public string Name { get; set; }
        public string Status { get; set; } = "To-Do";
        public DateTime? DeadlineAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Notes { get; set; }

    }
}
