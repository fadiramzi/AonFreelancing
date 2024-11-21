using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models.Entities
{
    [Table("Tasks")]
    public class TaskEntity
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public ProjectEntity Project { get; set; }
        public long ClientId { get; set; }
        public ClientEntity Client { get; set; }
        public string Name { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
