using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AonFreelancing.Models.DTOs
{
    public class UserDTO
    {

        //public int Id { get; set; }

        [StringLength(64)]
        public string Name { get; set; }

        [StringLength(32)]
        [MinLength(2, ErrorMessage = "يرجى ادخال  اسم صالح ")]
        public string Username { get; set; }

        [MinLength(4, ErrorMessage = "كلمة المرور قصيرة يرجى ادخال كلمة مرور اطول")]
        public string Password { get; set; }


    }

    public class UserOutDTO
    {
        public int Id { get; set; }

        [StringLength(64)]
        public string Name { get; set; }

        [StringLength(32)]
        [MinLength(2, ErrorMessage = "يرجى ادخال اسم اسم صالح ")]
        public string Username { get; set; }

    }
}
