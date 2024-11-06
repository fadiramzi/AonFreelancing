using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models
{
    [Table("Otp")]
    public class Otp
    {
        [Key]
        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be exactly 6 digits.")]
        public  string Code { get; set; } = string.Empty;
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
