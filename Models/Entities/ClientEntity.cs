using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models.Entities
{

    [Table("Clients")]
    public class ClientEntity : UserEntity
    {
        public string CompanyName { get; set; }
        public List<ProjectEntity>? Projects { get; set; }

    }
}
