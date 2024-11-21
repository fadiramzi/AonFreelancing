using System.ComponentModel.DataAnnotations.Schema;


namespace AonFreelancing.Models.Entities
{
    [Table("SystemUsers")]
    public class SystemUser : User
    {
        public string Permissions { get; set; }
    }
}
