using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models.Entities
{

    [Table("Clients")]
    public class Client : User
    {
        public string CompanyName { get; set; }
        public List<Project>? Projects { get; set; }

    }
}
