using Microsoft.AspNetCore.Http;

namespace AonFreelancing.Services
{
    public class FileVaildation
    {
        public const long MaxFileSizeInBytes = 1 * 1024 * 1024; // 1 MB
        public static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        public async Task<string> ValidateImageFileAsync(IFormFile file)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

            using FileStream stream = new FileStream(path + fileName, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }
    }
}
