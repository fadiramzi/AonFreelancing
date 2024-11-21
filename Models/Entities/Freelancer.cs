using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models.Entities
{
    [Table("Freelancers")]
    public class Freelancer : User
    {

        public string Skills { get; set; }

    }
}
