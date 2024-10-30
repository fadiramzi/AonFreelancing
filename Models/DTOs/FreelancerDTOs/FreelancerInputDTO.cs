using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using AonFreelancing.Models.DTOs.UserDTOs;

namespace AonFreelancing.Models.DTOs.FreelancerDTOs
{
    public class FreelancerInputDTO : UserDTO
    {
        [Required]
        [MinLength(4, ErrorMessage = "Skills length is too short.")]
        public string Skills { get; set; }
    }
}
