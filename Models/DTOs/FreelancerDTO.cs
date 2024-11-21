namespace AonFreelancing.Models.DTOs
{
    public class FreelancerDTO:UserDTO
    {

        public string Skills { get; set; }
    }

    public class FreelancerRequestDTO : UserDTO
    {
        public string Skills { get; set; }
    }

    public class FreelancerResponseDTO : UserResponseDTO { 
        public string? Skills { get; set; }
      
    }
}
