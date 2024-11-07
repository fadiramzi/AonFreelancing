using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{
    public record RegRequest(
        [Required, MinLength(2)] 
        string Name,
        [Required, MinLength(4)]
        string Username,
        [Required, Phone] 
        string PhoneNumber,
        [Required, MinLength(4, ErrorMessage = "Too short password")]
        string Password,
        [Required, AllowedValues("Freelancer", "Client")] 
        string UserType,
        string? Skills = null,
        string? CompanyName = null
    );
}
