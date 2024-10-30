using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class ClientDTO:UserOutDTO
    {
        [Required]
        [MinLength(4, ErrorMessage = "اسم الشركة غير صالح")]
        public string CompanyName { get; set; }

        // Has many projects, 1-m
        public IEnumerable<ProjectOutDTO> Projects { get; set; }
    }

    public class ClientInputDTO: UserDTO
    {
        [Required]
        [MinLength(4,ErrorMessage = "اسم الشركة غير صالح")]
        public string CompanyName { get; set; }
    }
}
