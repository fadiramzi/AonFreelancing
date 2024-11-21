using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models.Entities
{
    [Table("Freelancers")]
    public class FreelancerEntity : UserEntity
    {

        public string Skills { get; set; }

    }
}
