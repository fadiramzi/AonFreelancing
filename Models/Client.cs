using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AonFreelancing.Models
{

    [Table("Clients")]
    public class Client
    {
        [Key]
        public int Id { get; set; }
        public string? CompanyName { get; set; }
        public IEnumerable<Project>? Projects { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        public User User { get; set; }
    }
}
