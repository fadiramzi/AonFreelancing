using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class ClientOutputDTO : UserOutputDTO
    {
        public string CompanyName { get; set; }

        // Has many projects, 1-m
        public IEnumerable<ProjectOutputDTO>? Projects { get; set; }

        public ClientOutputDTO() { }
        public ClientOutputDTO(Client client)
        {
            Id = client.Id;
            Name = client.Name;
            Username = client.Username;
            CompanyName = client.CompanyName;
            Projects = client.Projects?.Select(p => new ProjectOutputDTO(p));
        }
    }

    public class ClientInputDTO : UserInputDTO
    {
        [Required]
        [MinLength(4, ErrorMessage = "Company Name is too short")]
        public string CompanyName { get; set; }
    }
}
