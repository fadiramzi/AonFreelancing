namespace AonFreelancing.Models.DTOs
{
    public class TaskInputDto
    {
        public string Name { get; set; }
        public DateTime DeadlineAt { get; set; }
        public string Notes { get; set; }
    }

    public class TaskStatusDto
    {
        public string NewStatus { get; set; }
    }

}
