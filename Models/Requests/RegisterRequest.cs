using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{
    public class RegRequest
    {
        [Required]
        [MinLength(2)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [MinLength(6, ErrorMessage ="Too short password")]
        public string Password { get; set; }
    }
}
