using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectOutDTO
    {
        
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public decimal Budget { get; set; }
        public int Duration { get; set; }
        public string PriceType { get; set; }
        public string QualificationName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreationTimeAgo { get; set; }

    }
}
