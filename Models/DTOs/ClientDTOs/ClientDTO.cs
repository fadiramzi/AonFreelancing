using System.ComponentModel.DataAnnotations;
using AonFreelancing.Models.DTOs.ProjectDTOs;
using AonFreelancing.Models.DTOs.UserDTOs;

namespace AonFreelancing.Models.DTOs.ClientDTO
{
    public class ClientDTO : UserOutDTO
    {
        public string CompanyName { get; set; }

        // Has many projects, 1-m
        public IEnumerable<ProjectOutDTO> Projects { get; set; }
    }

    public class ClientInputDTO : UserDTO
    {
        [Required]
        [MinLength(4, ErrorMessage = "Invalid Company Name")]
        public string CompanyName { get; set; }
    }
}
