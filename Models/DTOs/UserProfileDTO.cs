namespace AonFreelancing.Models.DTOs
{
    public class UserProfileDTO
    {
        public long id { get; set; }

        public string name { get; set; }
        public string CompanyName { get; set; }

        public string PhoneNumber { get; set; }
        public string Email { get; set; }   
        public List<Project> About {  get; set; }

    }
}
