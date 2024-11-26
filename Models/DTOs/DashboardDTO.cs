namespace AonFreelancing.Models.DTOs
{
    public class DashboardDTO
    {
        public class DashProjects
        { 
            public int? Total { get; set; }
            public int? Available { get; set; }
            public int? Closed { get; set; }
        }
        public class DashTasks
        {
            public int? Total { get; set; }
            public string? ToDo { get; set; } 
            public string? InReview { get; set; }
            public string? InProgress { get; set; }
            public string? Done { get; set; }
        }
    }
}
