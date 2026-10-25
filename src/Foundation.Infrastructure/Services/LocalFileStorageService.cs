using Foundation.Application.Abstractions;

namespace Foundation.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storageRoot;
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png",
        "image/jpeg",
        "image/jpg",
        "image/webp"
    };

    public LocalFileStorageService(string storageRoot)
    {
        _storageRoot = Path.GetFullPath(storageRoot);
        if (!Directory.Exists(_storageRoot))
        {
            Directory.CreateDirectory(_storageRoot);
        }
    }

    public async Task<string> SaveFileAsync(
        Stream stream,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (stream == null || stream.Length == 0)
        {
            throw new ArgumentException("Stream cannot be null or empty.", nameof(stream));
        }

        // Validate MIME type
        if (!AllowedMimeTypes.Contains(contentType))
        {
            throw new InvalidOperationException($"Content type '{contentType}' is not permitted.");
        }

        // Validate Magic Bytes
        if (!IsValidImageHeader(stream, contentType))
        {
            throw new InvalidOperationException("File content signature does not match declared MIME type.");
        }

        // Reset stream position after inspection
        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        // Safe extension mapping
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || extension is not (".png" or ".jpg" or ".jpeg" or ".webp"))
        {
            extension = contentType switch
            {
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => ".jpg"
            };
        }

        var safeStoredFileName = $"{Guid.NewGuid():N}{extension}";
        var destinationPath = Path.Combine(_storageRoot, safeStoredFileName);

        // Path Traversal Defense
        var fullPath = Path.GetFullPath(destinationPath);
        if (!fullPath.StartsWith(_storageRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid file destination path detected.");
        }

        using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
        await stream.CopyToAsync(fileStream, cancellationToken);

        return safeStoredFileName;
    }

    public Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var sanitized = Path.GetFileName(relativePath);
        var targetPath = Path.Combine(_storageRoot, sanitized);

        if (File.Exists(targetPath))
        {
            File.Delete(targetPath);
        }

        return Task.CompletedTask;
    }

    public Task<Stream?> GetFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var sanitized = Path.GetFileName(relativePath);
        var targetPath = Path.Combine(_storageRoot, sanitized);

        if (!File.Exists(targetPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        var stream = new FileStream(targetPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        return Task.FromResult<Stream?>(stream);
    }

    private static bool IsValidImageHeader(Stream stream, string contentType)
    {
        if (!stream.CanSeek || stream.Length < 12)
        {
            return true; // Stream cannot be validated by header peek; pass through
        }

        var header = new byte[12];
        stream.Seek(0, SeekOrigin.Begin);
        int read = stream.Read(header, 0, 12);
        stream.Seek(0, SeekOrigin.Begin);

        if (read < 4) return false;

        // PNG: 89 50 4E 47
        if (contentType.Equals("image/png", StringComparison.OrdinalIgnoreCase))
        {
            return header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47;
        }

        // JPEG: FF D8 FF
        if (contentType.StartsWith("image/jp", StringComparison.OrdinalIgnoreCase))
        {
            return header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
        }

        // WebP: RIFF ... WEBP
        if (contentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase))
        {
            return header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                   header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;
        }

        return true;
    }
}
