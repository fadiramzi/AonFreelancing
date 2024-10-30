using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs.ProjectDTOs
{
    public class ProjectInputDTO
    {
        public string Title { get; set; }

        [AllowNull]
        public string Description { get; set; }

        public int ClientId { get; set; }
        [AllowNull]
        public int? FreelancerId { get; set; }
    }
}
