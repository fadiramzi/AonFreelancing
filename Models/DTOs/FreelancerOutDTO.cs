namespace AonFreelancing.Models.DTOs
{
    public class FreelancerOutDTO:UserOutDTO
    {
        public string Skills { get; set; }
        public IEnumerable<ProjectOutDTO> Projects { get; set; }
    }
}
