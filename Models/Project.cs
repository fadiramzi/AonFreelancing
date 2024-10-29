using AonFreelancing.Models.DTOs;
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
        public string Title { get; set; }

        public string? Description { get; set; }

        public int ClientId { get; set; }//FK

        // Belongs to a client
        [ForeignKey("ClientId")]
        public Client Client { get; set; }

        public DateTime CreatedAt { get; set; }

        public Project() { }
        public Project(ProjectInputDTO projectInputDTO)
        {
            Title = projectInputDTO.Title;
            Description = projectInputDTO.Description;
            ClientId = projectInputDTO.ClientId;
        }

    }
}
