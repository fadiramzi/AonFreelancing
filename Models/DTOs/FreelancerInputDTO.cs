using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class FreelancerInputDTO: UserOutDTO
    {
        public string Skills { get; set; }

        [MinLength(4, ErrorMessage = "Too short password")]
        public string Password { get; set; }
    }
}
