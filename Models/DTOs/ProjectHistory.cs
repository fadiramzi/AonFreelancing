namespace AonFreelancing.Models.DTOs
{
    public class ProjectHistoryDTO
    {
        public DateTime EndDate { set; get; }
        public int ProjectId { set; get; }
    }
    public class ProjectHistoryOutDTO : ProjectOutDTO
    {
        public DateTime EndDate { set; get; }
    }
}