using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using static System.Net.Mime.MediaTypeNames;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectOutDTO
    {
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
        public string? Image { get; set; }
        public List<BidOutputDTO> Bids { get; set; }
        public ProjectOutDTO() { }
        public ProjectOutDTO(Project project, string imageBaseUrl) {
            Title = project.Title;
            Description = project.Description;
            Status = project.Status;
            Budget = project.Budget;
            if (project.ImageFileName != null)
                Image = $"{imageBaseUrl}/{project.ImageFileName}";
            Duration = project.Duration;
            PriceType = project.PriceType;
            Qualifications = project.QualificationName;
            CreatedAt = project.CreatedAt;
            StartDate = project.StartDate;
            EndDate = project.EndDate;
            Bids = project.Bids?.Select(b=>new BidOutputDTO(b)).OrderBy(b => b.ProposedPrice).ToList();
        }
    }
}
