using AonFreelancing.Models.DTOs;

namespace AonFreelancing.Models.Responses
{
    public class LoginResponse
    {
        public UserDetailsDTO UserDetailsDTO { get; set; }
        public string AccessToken { get; set; }
    }
}