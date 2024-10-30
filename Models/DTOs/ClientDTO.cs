using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AonFreelancing.Models.DTOs
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
    //to return Client info without projects in [HttpGet("BasicGetById")] endpoint
    public class ClientBasicDTO : UserOutDTO
    {
        public string CompanyName { get; set; }

    }

    public class ClientOutDTO : UserOutDTO
    {

        public string CompanyName { get; set; }

        //  Freelancer Has many projects, 1-m.
        public IEnumerable<ProjectOutDTO> Projects { get; set; }
    }
}
