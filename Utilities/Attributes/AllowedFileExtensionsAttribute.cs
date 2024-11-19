using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Utilities.Attributes;

public class AllowedFileExtensionsAttribute(string[] extensions) 
    : ValidationAttribute
{
    private string _extension;
    private new string ErrorMessage { get; } = "This file extension is not allowed.";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not IFormFile file)
        {
            return ValidationResult.Success;
        }
        _extension = Path.GetExtension(file.FileName);
        if (FileCheck.CheckFileSignature(file.FileName, file.OpenReadStream(), extensions))
        {
            return ValidationResult.Success;
        }
        
        return new ValidationResult(ErrorMessage);
    }
}