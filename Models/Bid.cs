using AonFreelancing.Models.DTOs;
using AonFreelancing.Utilities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models
{
    [Table("Bids")]
    public class Bid
    {
        public long Id { get; set; }
        public long ProjectId {  get; set; }
        public long FreelancerId {  get; set; }
        public decimal ProposedPrice {  get; set; }
        public string Notes {  get; set; }
        public string Status {  get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ApprovedAt{ get; set; }
        
        public Bid() { }
        public Bid(BidInputDTO bidRequest,long projectId, long freelancerId)
        {
            SubmittedAt = DateTime.Now;
            ProposedPrice = bidRequest.ProposedPrice;
            Notes = bidRequest.Notes;
            ProjectId = projectId;
            FreelancerId = freelancerId;
            Status = Constants.BID_STATUS_PENDING;
        }

    }
}
