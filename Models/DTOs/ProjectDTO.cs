using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectInputDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }//Number of days
        public string PriceType { get; set; }
        public decimal Budget { get; set; }
        public DateTime CreatedAt { get; set; }
        public string QualificationName { get; set; }
        public string Status { get; set; }
    }

    public class ProjectResponseDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }//Number of days
        public string PriceType { get; set; }
        public decimal Budget { get; set; }
        public DateTime CreatedAt { get; set; }
        public string QualificationName { get; set; }
        public string Status { get; set; }
        public long ClientId { get; set; }

    }
    public class ProjectOutDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
