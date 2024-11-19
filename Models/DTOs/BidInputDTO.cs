using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class BidInputDTO
    {
        [Range(0, int.MaxValue)]
        public decimal ProposedPrice { get; set; }
        public string Notes { get; set; }
    }
}
