using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests;

public record PhoneNumberReq(
    [Required, StringLength(14, MinimumLength = 14)] 
    string PhoneNumber
);