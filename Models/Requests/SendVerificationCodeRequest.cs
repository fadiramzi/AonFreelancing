using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{
    public class SendVerificationCodeRequest
    {

        [Required]
        [Length(14,14)]
        public string PhoneNumber {  get; set; }
    }
}
