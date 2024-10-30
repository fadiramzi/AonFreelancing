using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class FreelancerDTO:UserOutDTO
    {

        public IEnumerable<ProjectOutDTO> Projects { get; set; }

        [MinLength(2, ErrorMessage = "يرجى ادخال مهارات حقيقية")]
        public string Skills { get; set; }
    }
}
