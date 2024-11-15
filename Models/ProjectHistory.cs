using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models
{
    [Table("ProjectHistory")]
    public class ProjectHistory
    {
        [Key]
        public int Id { set; get; }
        [Required]
        public DateTime EndDate { set; get; }
        public int ProjectId { set; get; }
        [ForeignKey("ProjectId")]
        public Project Project { set; get; }
    }
}