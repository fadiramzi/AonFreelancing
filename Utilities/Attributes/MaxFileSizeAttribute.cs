using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Utilities.Attributes;

public class MaxFileSizeAttribute(int maxFileSize) : ValidationAttribute
{
    private new string ErrorMessage { get; } = $"File size exceeds {maxFileSize} bytes.";
    
    protected override ValidationResult? IsValid(object? value, 
        ValidationContext validationContext)
    {
        return value switch
        {
            null => throw new ArgumentNullException(nameof(value)),
            IFormFile file when file.Length > maxFileSize => new ValidationResult(ErrorMessage),
            _ => ValidationResult.Success
        };
    }
}