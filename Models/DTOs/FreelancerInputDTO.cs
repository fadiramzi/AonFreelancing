using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs
{
    public class FreelancerInputDTO : UserInputDTO
    {
        [Required]
        public string Skills { get; set; }
    }
}
