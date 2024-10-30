using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AonFreelancing.Models
{
    [Table("SystemUsers")]
    public class SystemUser
    {
        [Key]
        public int Id { get; set; }
        public string Permissions { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
