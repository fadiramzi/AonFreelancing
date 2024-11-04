using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{
    public class AuthRequest
    {
        [Required]
        [MinLength(4, ErrorMessage ="Invalid Username")]
        public string UserName { get; set; }
        [Required]
        [MinLength(4, ErrorMessage = "Invalid Password")]
        public string Password { get; set; }
    }
}
