using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AonFreelancing.Models
{
    [Table("Freelancers")]
    public class Freelancer
    {
        [Key]
        public int Id { get; set; }
        public string? Skills { get; set; }
        public void DisplayProfile()
        { }
        public IEnumerable<Project>? Projects { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        public User User { get; set; }
    }
}
