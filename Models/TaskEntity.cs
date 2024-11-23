namespace AonFreelancing.Models
{
    public class TaskEntity
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public Project Project { get; set; }
        public string Name { get; set; }
        public string Status { get; set; } = "To-Do";
        public DateTime? DeadlineAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Notes { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

    }
}
