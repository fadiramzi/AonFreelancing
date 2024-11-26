namespace AonFreelancing.Models.DTOs
{
    public class StatisticsResponseDTO
    {
        public ProjectStatistics Projects { get; set; }
        public TaskStatistics Tasks { get; set; }
    }

    public class ProjectStatistics
    {
        public int Total { get; set; }
        public int Available { get; set; }
        public int Closed { get; set; }
    }

    public class TaskStatistics
    {
        public int Total { get; set; }
        public string ToDo { get; set; }
        public string InReview { get; set; }
        public string InProgress { get; set; }
        public string Done { get; set; }
    }

}
