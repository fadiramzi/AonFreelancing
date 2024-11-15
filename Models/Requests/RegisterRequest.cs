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

        [Required]
        [AllowedValues("Freelancer","Client")]
        public string UserType { get; set; }

        // For freelancer type only
        public string? Skills { get; set; }

        // For Client user type only


        public string? CompanyName { get; set; }

    }
}
