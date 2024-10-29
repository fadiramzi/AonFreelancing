namespace AonFreelancing.Models.DTOs
{
    public class FreelancerOutputDTO : UserOutputDTO
    {
        public string Skills { get; set; }
        public FreelancerOutputDTO(Freelancer freelancer)
        {
            Id = freelancer.Id;
            Username = freelancer.Username;
            Name = freelancer.Name;
            Skills = freelancer.Skills;
        }
    }
}
