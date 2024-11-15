using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectOutDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public int ClientId { get; set; }//FK

        public DateTime CreatedAt { get; set; }
    }

    public class ProjectHistoryDTO
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ProjectFeedDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public decimal Budget { get; set; }
        public int Duration { get; set; }
        public string Qualifications { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreationTimeAgo => GetTimeAgo(CreationDate);

        private string GetTimeAgo(DateTime creationDate)
        {
            var timespan = DateTime.Now - creationDate;
            // Custom formatting based on timespan (e.g., "1 hour ago")
            if (timespan.TotalHours < 1)
                return $"{timespan.Minutes} minutes ago";
            if (timespan.TotalDays < 1)
                return $"{timespan.Hours} hours ago";
            return $"{timespan.Days} days ago";
        }
    }
}
