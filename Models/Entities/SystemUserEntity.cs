using System.ComponentModel.DataAnnotations.Schema;


namespace AonFreelancing.Models.Entities
{
    [Table("SystemUsers")]
    public class SystemUserEntity : UserEntity
    {
        public string Permissions { get; set; }
    }
}
