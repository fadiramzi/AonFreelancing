using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class FreeelnacerFeedInputDTO
    {
        [AllowedValues(["Month", "Year"])]
        public string DurationType { get; set; }
        [Range(1, 30)]
        public int DurationPeriod { get; set; }

        [Range(0.01, 1000  , ErrorMessage = "MinPrice must be greater than 0.")]
        public decimal MinPrice { get; set; }

        [Range(0.01, 1000, ErrorMessage = "MaxPrice must be lower than 1000000.")]
        public decimal MaxPrice { get; set; }
    }
}
