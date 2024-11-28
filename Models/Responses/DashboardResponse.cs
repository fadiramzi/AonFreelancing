using AonFreelancing.Models.DTOs;

namespace AonFreelancing.Models.Responses
{
    public class DashboardResponse
    {
        public DashprojectsDTO Projects { get; set; }
        public DashTasksDTO Tasks { get; set; }
    }
}
