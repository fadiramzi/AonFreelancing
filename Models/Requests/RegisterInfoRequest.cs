using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{

    public class RegisterInfoRequest
    {
        [Required]
        [MinLength(2)]
        public string Name { get; set; }

        [Required]
        [MinLength(4)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
       public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }


        [Required]
        [MinLength(6, ErrorMessage ="Too short password")]
        public string Password { get; set; }

        [Required]
        [AllowedValues("Freelancer","Client")]
        public string UserType { get; set; }

        public string? Skills { get; set; }

        public string? CompanyName { get; set; }

        public string? About { get; set; }

    }
}
