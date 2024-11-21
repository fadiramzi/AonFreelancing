using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{
    public class TaskRq
    {
        [AllowedValues("to_do", "in_progress", "in _review", "done")]
        public string Name { get; set; }
        public DateTime DeadLine { get; set; }
    }
}
