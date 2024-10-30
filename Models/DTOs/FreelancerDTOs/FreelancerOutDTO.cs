using AonFreelancing.Models.DTOs.ProjectDTOs;
using AonFreelancing.Models.DTOs.UserDTOs;

namespace AonFreelancing.Models.DTOs.FreelancerDTOs
{
    public class FreelancerOutDTO : UserOutDTO
    {
        public string? Skills { get; set; }
        public IEnumerable<Project> Projects { get; set; }

    }
}
