namespace AonFreelancing.Models.DTOs
{
    public class BidOutputDTO
    {
        public long Id { get; set; }
        //public long ProjectId { get; set; }
        public long FreelancerId { get; set; }
        public decimal ProposedPrice { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public BidOutputDTO() { }
        public BidOutputDTO(Bid bid) 
        {
            Id = bid.Id;
            FreelancerId = bid.FreelancerId;
            ProposedPrice = bid.ProposedPrice;
            Notes = bid.Notes;
            Status = bid.Status;
            SubmittedAt = bid.SubmittedAt;
            ApprovedAt = bid.ApprovedAt;
        }
    }
}
