using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{
    public record VerifyPhoneNumberReq(
        [Required, StringLength(14, MinimumLength = 14)] 
        string Phone,
        [Required, StringLength(6, MinimumLength = 6)] 
        string Otp
    );
}
