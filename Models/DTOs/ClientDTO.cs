using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class ClientDTO:UserOutDTO
    {
        public string? CompanyName { get; set; }

        // Has many projects, 1-m
        public IEnumerable<ProjectOutDTO>? Projects { get; set; }
    }

    public class ClientInputDTO: UserOutDTO
    {
        [Required]
        [MinLength(4,ErrorMessage ="Invalid Company Name")]
        public string? CompanyName { get; set; }
        //public IEnumerable<ProjectOutDTO> Projects { get; set; }  // for testing.
        public string? Password { get; set; }

    }
}
