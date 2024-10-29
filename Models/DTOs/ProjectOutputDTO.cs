using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectOutputDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public int ClientId { get; set; }//FK
        public DateTime CreatedAt { get; set; }

        public ProjectOutputDTO() { }   
        public ProjectOutputDTO(Project project)
        {
            Id = project.Id;
            Title = project.Title;
            Description = project.Description;
            ClientId = project.ClientId;
            CreatedAt = project.CreatedAt;
        }
    }
}
