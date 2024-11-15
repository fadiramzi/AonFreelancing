using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace AonFreelancing.Models
{
    //Entity
    [Table("Projects")]
    public class Project
    {

        public int Id { get; set; }

        [Required]
        [MaxLength(512)]
        public string Title { get; set; }

        [AllowNull]
        [MaxLength(1024)]
        public string Description { get; set; }
        

        public DateTime CreatedAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [AllowedValues("PerHour","Fixed")]
        public string PriceType { get; set; }

        public int Duration { get; set; } // int Days

        [AllowedValues("UI/UX","Front-end","Back-end","Mobile","Full-stack")]
        public string QualificationName { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Budget must be a positive value")]
        public decimal Budget {  get; set; }

        [AllowedValues("Available", "Closed")]
        public string Status { get; set; } = "Available";

        // Relations
        
        public long ClientId { get; set; }//FK

        
        [ForeignKey("ClientId")]
        [JsonIgnore]
        public Client Client { get; set; }

        [AllowNull]
        public long? FreelancerId { get; set; } //FK

        [AllowNull]
        [ForeignKey("FreelancerId")]
        [JsonIgnore]
        public Freelancer Freelancer { get; set; }



    }
}
