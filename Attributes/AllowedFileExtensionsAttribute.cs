using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Attributes
{
    public class AllowedFileExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;
        private string _extension;
        public AllowedFileExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            if (value is not IFormFile file)
                return ValidationResult.Success;

            _extension = Path.GetExtension(file.FileName);
            if (Utilities.FileCheckUtil.IsValidFileExtensionAndSignature(file.FileName, file.OpenReadStream(), _extensions))
                return ValidationResult.Success;

            return new ValidationResult(GetErrorMessage());
        }

        public string GetErrorMessage()
        {
            return $"either file extension ({_extension}) is not allowed or the file has been corrupted";
        }
    }
}