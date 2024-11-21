using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models.Entities
{
    [Table("Otps")]
    public class OtpEntity
    {
        [Key]
        public string PhoneNumber { get; set; }

        public string Code { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }

    }
}
