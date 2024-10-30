using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AonFreelancing.Models.DTOs
{
    public class UserDTO
    {
        [StringLength(64)]
        [Required]
        public string Name { get; set; }

        [StringLength(32)]
        [Required]
        public string Username { get; set; }

        [MinLength(4,ErrorMessage ="Too short password")]
        [Required]
        public string Password { get; set; }
    }

    public class UserOutDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }

    }
}
