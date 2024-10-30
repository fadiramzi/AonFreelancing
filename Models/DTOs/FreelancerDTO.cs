using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class FreelancerDTO:UserOutDTO
    {
        public string Skills { get; set; }
        public IEnumerable<ProjectOutDTO>? Projects { get; set; }
    }
    
    public class FreelancerInputDTO : UserDTO
    {
        [Required]
        public string Skills { get; set; }
    }
}
