using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models
{
    public class EntityTask
    {

        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }
        public int ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public Project Project { get; set; }

        [AllowedValues("to_do", "in_progress", "in _review", "done")]
        public string status { get; set; } = "to_do";


        public DateTime DedlineAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public string? Notes { get; set; }
    }
}
