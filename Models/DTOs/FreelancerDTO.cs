namespace AonFreelancing.Models.DTOs
{
    public class FreelancerDTO: UserOutDTO
    {
       
        public string Skills { get; set; }

    }
    public class FreelancerInputDTO : UserDTO
    {
       
        public string Skills { get; set; }

    }
    public class FreelancerOutDTO : UserOutDTO
    {
        // Has many projects, 1-m
        public IEnumerable<Project>? Projects { get; set; }
        public string Skills { get; set; }

    }
}
