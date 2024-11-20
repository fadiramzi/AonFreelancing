using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectOutDTO
    {
<<<<<<< HEAD
        
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public decimal Budget { get; set; }
        public int Duration { get; set; }
        public string PriceType { get; set; }
        public string QualificationName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreationTimeAgo { get; set; }

=======
        public int Id { get; set; }
        public int Duration {  get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Qualifications { get; set; }
        public string PriceType { get; set; }
        public string Status {  get; set; }
        public decimal Budget {  get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartDate {  get; set; }
        public DateTime? EndDate { get; set; }
        public string? CreationTime {  get; set; }
>>>>>>> bd49e789eca82cf0b70e0aad4d121920d1c2c3b2
    }
}
