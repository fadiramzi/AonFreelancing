using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{
    public class ResetPasswordReq
    {
        [Required]
        [MinLength(6, ErrorMessage = "Too short password")]
        public string? Password { get; set; }

        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        [Required]
        [Phone]
        [Length(14, 14)]
        public string? PhoneNumber { get; set; }

        [Required]
        public string? Token { get; set; }
    }
}
