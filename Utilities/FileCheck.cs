using System.ComponentModel;

namespace AonFreelancing.Utilities;

public class FileCheck
{
    private static readonly Dictionary<string, List<byte[]>> FileSignature = new()
    {
        { ".gif", [new byte[] { 0x47, 0x49, 0x46, 0x38 }] },
        { ".png", [new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }] },
        { ".jpeg", [
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 }
            ]
        },
        { ".jpg", [
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }
            ]
        },
    };

    public static bool CheckFileSignature(string fileName, Stream stream, string[] extensions)
    {
        if (string.IsNullOrEmpty(fileName) || stream.Equals(null) || stream.Length == 0) return false;
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !extensions.Contains(extension)) return false;
        
        stream.Position = 0;
        using var reader = new BinaryReader(stream);
        var signatures = FileSignature[extension];
        var bytes = reader.ReadBytes(signatures.Max(entry => entry.Length));
        
        return signatures.Any(signature => bytes.Take(signature.Length).SequenceEqual(signature));
    }
}