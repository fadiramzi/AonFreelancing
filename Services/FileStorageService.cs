namespace AonFreelancing.Services;

public class FileStorageService
{
    private static readonly string Root = Path.Combine(Directory.GetCurrentDirectory(),"Uploads");

    public async Task<string> SaveFileAsync(IFormFile file)
    {
        var filePath = $"{Guid.NewGuid().ToString()}{Path.GetExtension(file.FileName)}";
        var stream = File.Create(Path.Combine(Root, filePath));
        await file.CopyToAsync(stream);
        return filePath;
    }
}