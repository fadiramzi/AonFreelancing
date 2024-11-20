using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class TaskInputDTO
    {
        [MinLength(3,ErrorMessage ="task name is too short, it should contain 3 or more characters")]
        public string Name {  get; set; }
        public string? Notes {  get; set; }
        public DateTime DeadlineAt{ get; set; }
    }
}
