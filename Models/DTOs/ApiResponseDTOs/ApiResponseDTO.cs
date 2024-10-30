using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs.ResponseDTOs
{
    public class ApiResponseDTO <T>
    {
        [Required]
        public bool IsSuccess { get; set; }
        public T? Results { get; set; }
        public Error? Error { get; set; }
    }
}
