using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AonFreelancing.Models.DTOs
{
    public class UserDTO
    {
        [StringLength(64)]
        public string? Name { get; set; }

        [StringLength(32)]
        public string? Username { get; set; }

        [MinLength(4,ErrorMessage ="Too short password")]
        public string? Password { get; set; }

    }

    public class UserOutDTO
    {
        [Key]
        [JsonIgnore]
        public int Id { get; set; }
        public string? Name { get; set; }

        public string? Username { get; set; }

    }

}
