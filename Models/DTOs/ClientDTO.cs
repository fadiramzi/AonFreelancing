using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class ClientDTO:UserDTO
    {
        public string? CompanyName { get; set; }

        //public IEnumerable<ProjectOutDTO>? Projects { get; set; } // for fast testing.
    }

    public class ClientOutDTO: UserOutDTO
    {
        [Required]
        [MinLength(4,ErrorMessage ="Invalid Company Name")]
        public string? CompanyName { get; set; }
        public IEnumerable<ProjectOutDTO>? Projects { get; set; }  

    }
}
