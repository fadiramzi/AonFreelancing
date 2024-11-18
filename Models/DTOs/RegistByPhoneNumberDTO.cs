
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
namespace AonFreelancing.Models.DTOs

{
    public class RegistByPhoneNumberDTO
    {

        [Required]
        public string PhoneNumber { get; set; }

    }
}
