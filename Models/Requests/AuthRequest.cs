using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{
    public record AuthRequest(
        [Required, MinLength(4, ErrorMessage = "Invalid Username")] 
        string UserName,
        [Required, MinLength(4, ErrorMessage = "Invalid Password")] 
        string Password
    );
}
