using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{
    public class TaskStatusRq
    {

        [AllowedValues]
        public string newStatus { get; set; }
    }
}
