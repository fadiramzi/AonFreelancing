using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models
{
    //Entity
    [Table("Projects")]
    public class Project
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(512)]
        public string Title { get; set; }
        [AllowNull]
        [MaxLength(1024)]
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        [Required]
        [AllowedValues("PerHour", "Fixed")]
        public string PriceType { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Budget {  get; set; }
        public int Duration { get; set; }
        [AllowedValues("backend", "Ui/Ux", "mobile", "full stack", "frontend")]
        public string QualificationName { get; set; }
        [AllowedValues("Closed", "Available")]
        public string Status { get; set; }

        public long ClientId { get; set; }
        [ForeignKey("ClientId")]
        public Client Client { get; set; }

        [AllowNull]
        public long? FreelancerId { get; set; }
        [AllowNull]
        [ForeignKey("FreelancerId")]
        public Freelancer Freelancer { get; set; }

        public ProjectHistory ProjectHistory { get; set; }
    } 
}
