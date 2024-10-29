using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs
{
    public class FreelancerInputDTO : UserDTO
    {
        [Required]
        [MinLength(4, ErrorMessage = "Skills length is too short.")]
        public string Skills { get; set; }
    }
}
