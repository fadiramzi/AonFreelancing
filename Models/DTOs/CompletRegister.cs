using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class CompletRegister
    {

        [Required]
        public string Name { get; set; }


        [EmailAddress]
        [Required]
        public string EmailAddress { get; set; }
        [Required]
        public string Password { get; set; } 
        [Required]
        public string PhoneNumber { get; set; }

    }
}
