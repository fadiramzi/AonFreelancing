using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AonFreelancing.Utilities;

namespace AonFreelancing.Models.Entities;

[Table("TempUser")]
public class TempUser
{
    [Key]
    public long Id { get; set; }
    [Required]
    public string PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    [Required, AllowedValues(Constants.USER_TYPE_FREELANCER, Constants.USER_TYPE_CLIENT)]
    public string UserType { get; set; }
}