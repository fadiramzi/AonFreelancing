using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AonFreelancing.Models.DTOs
{
    public class UserInputDTO
    {
        [Required]
        [StringLength(64)]
        public string Name { get; set; }

        [Required]
        [StringLength(32)]
        public string Username { get; set; }

        [Required]
        [MinLength(4,ErrorMessage ="Too short password")]
        public string Password { get; set; }


    }

    public class UserOutputDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Username { get; set; }

    }
}
