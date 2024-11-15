using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models
{
    public class UserTemp
    {
        [Required]
        [Key]
        public string PhoneNumber { get; set; }

        [Required]
        public Boolean IsActive { get; set; }
    }
}
