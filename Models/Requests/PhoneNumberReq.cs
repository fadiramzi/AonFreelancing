using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests;

public class PhoneNumberReq
{
    [Required, StringLength(14, MinimumLength = 14)]
    [Phone]
    public string PhoneNumber { get; set; }


}