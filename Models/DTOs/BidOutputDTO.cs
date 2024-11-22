namespace AonFreelancing.Models.DTOs
{
    public class BidOutputDTO
    {
        public decimal ProposedPrice { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
