using AonFreelancing.Contexts;


namespace AonFreelancing.Models.Services
{
    public class FileService
    {
       public static readonly string _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        private readonly MainAppContext mainAppContext;
        public FileService(MainAppContext _mainappcontext)
        {
            mainAppContext = _mainappcontext;
        }
        public async Task<string> SaveAsync(IFormFile formFile)
        {
            string filePath = $"{Guid.NewGuid().ToString()}{Path.GetExtension(formFile.FileName)}";
            using Stream stream = File.Create(Path.Combine(_uploadFolder, filePath));
            await formFile.CopyToAsync(stream);
            return filePath;
        }
    }
    }

