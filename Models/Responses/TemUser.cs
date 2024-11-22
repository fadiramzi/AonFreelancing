using System.ComponentModel.DataAnnotations;
using Twilio.Types;

namespace AonFreelancing.Models.Responses
{
    public class TemUser
    {

        [Key]
   public long Id { get; set; }
        public string? phoneNumber { get; set; }

        public bool PhoneNumberConfirm { get; set; }
     

    }
}
