using AonFreelancing.Utilities;
using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{
    public record TasksStatusRequest(
        [Required(ErrorMessage ="This value is required.")]
        [AllowedValues(Constants.TASK_STATUS_TODO, Constants.TASK_STATUS_IN_PROGRESS,
        Constants.TASK_STATUS_IN_REVIEW, Constants.TASK_STATUS_DONE, ErrorMessage = "This value is not allowed.")]
        string Status
    );
}
