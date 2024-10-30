using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class ClientUpdateDto : UserDTO
    {
        [MinLength(2, ErrorMessage = "يرجى ادخال اسم شركة صالح")]
        public string CompanyName { get; set; }
    }
}
