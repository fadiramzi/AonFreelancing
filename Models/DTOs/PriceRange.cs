using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class PriceRange:IValidatableObject 
    {
        [Range(0, int.MaxValue)]
        public decimal? MinPrice { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public decimal? MaxPrice { get; set; } = int.MaxValue;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MinPrice >= MaxPrice)
                yield return new ValidationResult("MinPrice must be less than MaxPrice");
        }
        
        
    }
}
