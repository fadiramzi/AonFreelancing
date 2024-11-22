namespace AonFreelancing.Models.DTOs
{
    public class ProjectBidsDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Budget { get; set; }
        public string Status { get; set; }
        public int Duration { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<BidOutputDTO> Bids { get; set; }
    }
}
