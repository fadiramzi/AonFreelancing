using System.Reflection.Metadata.Ecma335;

namespace AonFreelancing.Models.DTOs
{
    public class UserStatisticsDTO
    {
        public ProjectsStatisticsDTO Projects { get; set; }
        public TasksStatisticsDTO Tasks { get; set; }

        public UserStatisticsDTO(ProjectsStatisticsDTO projectsStatistics, TasksStatisticsDTO tasksStatistics)
        {
            Projects = projectsStatistics;
            Tasks = tasksStatistics;
        }
    }
}
