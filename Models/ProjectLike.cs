using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models
{
    [Table("ProjectLikes")]
    public class ProjectLike
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public long UserId {  get; set; }
        public DateTime CreatedAt {  get; set; }

        public ProjectLike(long userId, long projectId)
        {
            UserId = userId;
            ProjectId = projectId;
            CreatedAt = DateTime.Now;
        }

    }
}
