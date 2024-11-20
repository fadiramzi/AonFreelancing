using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{
<<<<<<< HEAD:Models/Requests/RegisterInfoRequest.cs

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
=======
    public record RegisterRequest(
        [Required, MinLength(2)] 
        string Name,
        [Required, MinLength(4)]
        string Username,
        [Required, Phone] 
        string PhoneNumber,
        [Required, MinLength(4, ErrorMessage = "Too short password")]
        string Password,
        [Required, AllowedValues("FREELANCER", "CLIENT")] 
        string UserType,
        string? Skills = null,
        string? CompanyName = null
    );
>>>>>>> bd49e789eca82cf0b70e0aad4d121920d1c2c3b2:Models/Requests/RegisterRequest.cs
}
