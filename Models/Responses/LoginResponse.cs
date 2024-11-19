using AonFreelancing.Models.DTOs;
using System.Text.Json.Serialization;

namespace AonFreelancing.Models.Responses
{
    public class LoginResponse
    {
        [JsonPropertyName("UserDetails")]
        public UserDetailsDTO UserDetailsDTO { get; set; }
        public string AccessToken { get; set; }
    }
}
