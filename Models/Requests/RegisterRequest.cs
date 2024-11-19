using System.ComponentModel.DataAnnotations;
using AonFreelancing.Utilities;

namespace AonFreelancing.Models.Requests
{
    public record RegisterRequest(
        [Required, MinLength(2)] 
        string Name,
        [Required, MinLength(4)]
        string Username,
        [EmailAddress(ErrorMessage = "Invalid Email Address"), MinLength(2)]
        string Email,
        [Required, Phone] 
        string PhoneNumber,
        [Required, MinLength(4, ErrorMessage = "Too short password")]
        string Password,
        string? Skills = null,
        string? CompanyName = null
    );
}
