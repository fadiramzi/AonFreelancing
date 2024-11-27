using AonFreelancing.Utilities;
using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class TaskInputDTO
    {
        public string Name { get; set; }
        public DateTime DeadlineAt { get; set; }
        public string Notes { get; set; }
    }

    public class TaskStatusDto
    {
        [AllowedValues(Constants.TASK_STATUS_DONE, Constants.TASK_STATUS_IN_REVIEW, Constants.TASK_STATUS_IN_PROGRESS, Constants.TASK_STATUS_TO_DO)]
        public string NewStatus { get; set; }
    }

}
