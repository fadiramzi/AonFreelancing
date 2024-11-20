namespace AonFreelancing.Services;

public class FileStorageService
{
    public static readonly string Root = Path.Combine(Directory.GetCurrentDirectory(),"uploads");

    public async Task<string> SaveFileAsync(IFormFile file)
    {
        var filePath = $"{Guid.NewGuid().ToString()}{Path.GetExtension(file.FileName)}";
        using var stream = File.Create(Path.Combine(Root, filePath));
        await file.CopyToAsync(stream);
        return filePath;
    }
}