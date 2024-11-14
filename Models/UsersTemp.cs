using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models
{
    [Table("UsersTemp")]
    public class UsersTemp
    {
        [Key]
        [Required]
       public string PhoneNumber { get; set; }
        [AllowNull]
       public bool? IsVerfied { get; set; }
        [AllowNull]
        public DateTime? VerifyTime { get; set; }

    }
}
