using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models
{
    [Table("Otps")]
    public class OTP
    {
        [Key]
        public string PhoneNumber { get; set; }

        public string Code { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }

    }
}
