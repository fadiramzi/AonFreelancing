using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class FreelancerDTO:UserDTO
    {
        public string? Skills { get; set; }
        //public IEnumerable<ProjectOutDTO>? Projects { get; set; }  // for fast testing
    }

    public class FreelancerOutDTO : UserOutDTO
    {
        public string? Skills { get; set; }

        [MinLength(4, ErrorMessage = "Too short password")]

        // has many - 1 to  m
        public IEnumerable<ProjectOutDTO>? Projects { get; set; }
    }
}

