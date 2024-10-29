using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectInputDTO
    {
        public string Title { get; set; }

        [AllowNull]
        public string Description { get; set; }

        
        //[ForeignKey("ClientId")]
        public int ClientId { get; set; }//FK
    }
}
