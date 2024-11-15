using static System.Runtime.InteropServices.JavaScript.JSType;
using Twilio.Types;
using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models
{
    public class OTP
    {
        [Key]
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        
        // Code 6 digits
        public string Code { get; set; }
        
        // date of record insertion
        public DateTime CreatedAt { get; set; }
        
        // date of expiration, 5 - 10 minutes
        public DateTime ExpireAt { get; set; }

        // Boolean to indicate if this sent OTP used or not
        public bool IsUsed { get; set; }
    }
}
