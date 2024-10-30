namespace AonFreelancing.Models.DTOs
{
    public class FreelancerDTO:UserDTO
    {

        public string Skills { get; set; }

        //  Freelancer Has many projects, 1-m.
        public IEnumerable<ProjectOutDTO> Projects { get; set; }
    }
    //to return freelancer info without projects in [HttpGet("BasicGetById")] endpoint
    public class FreelancerBasicDTO : UserOutDTO
    {
        public string Skills { get; set; }

    }
    public class FreelancerInDTO : UserDTO
    {
        public string Skills { get; set; }

    }
    public class FreelancerOutDTO : UserOutDTO
    {

        public string Skills { get; set; }

        //  Freelancer Has many projects, 1-m.
        public IEnumerable<ProjectOutDTO> Projects { get; set; }
    }
}
