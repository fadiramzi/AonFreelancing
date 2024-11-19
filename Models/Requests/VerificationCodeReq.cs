using System.ComponentModel.DataAnnotations;
using AonFreelancing.Utilities;

namespace AonFreelancing.Models.Requests;

public record VerificationCodeReq(
    [Required, StringLength(14, MinimumLength = 14)] 
    string PhoneNumber,
    [Required, AllowedValues(Constants.USER_TYPE_CLIENT, Constants.USER_TYPE_FREELANCER)] 
    string UserType
);