using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class FreelancerInputDto: UserDTO
    {
        public string Skills { get; set; }

        //public IEnumerable<ProjectOutDTO> Projects { get; set; }
    }

    public class FreelancerUpdateDto 
    {
        [StringLength(64)]
        public string Name { get; set; }

        [StringLength(32)]

        [MinLength(2, ErrorMessage = "يرجى ادخال اسم اسم صالح ")]
        public string Username { get; set; }

        [StringLength(255)]
        public string Skills { get; set; }

        public string password { get; set; }

        
    }


}
