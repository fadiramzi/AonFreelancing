using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class FreelancerDTO:UserDTO
    {

        public string Skills { get; set; }
    }
    public class FreelancerOutDTO : UserOutDTO
    {
        public string Skills { get; set; }
        public string Title { get; internal set; }
        public string Description { get; internal set; }
    }

    public class FreelancerLoadProject
    {
        [Required(ErrorMessage = "The query is mandatory !")]
        [Range(0, 1, ErrorMessage = "The query must be 0 or 1 !")]
        public int? LoadProjects { get; set; }
    }
}
