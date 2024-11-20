using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{
<<<<<<< HEAD
    public class VerifyReq
    {

        [Required]
        [Length(14, 14)]
        public string Phone { get; set; }

        [Required]
        [Length(6, 6)]
        public string Otp { get; set; }
    }
=======
    public record VerifyReq(
        [Required, StringLength(14, MinimumLength = 14)] 
        string Phone,
        [Required, StringLength(6, MinimumLength = 6)] 
        string Otp
    );
>>>>>>> bd49e789eca82cf0b70e0aad4d121920d1c2c3b2
}
