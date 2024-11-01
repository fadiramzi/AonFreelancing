using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{
    public class VerifyReq
    {

        [Required]
        [Length(14,14)]
        public string Phone {  get; set; }

        [Required]
        [Length(6,6)]
        public string Otp {  get; set; }
    }
}
