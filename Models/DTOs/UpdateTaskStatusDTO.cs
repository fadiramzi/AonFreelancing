using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class UpdateTaskStatusDTO
    {
        [Required]
        [AllowedValues("to-do", "in-progress", "in-review", "done")]
        public string NewStatus { get; set; }
    }
}
