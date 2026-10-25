namespace Foundation.Application.Abstractions;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream stream, string originalFileName, string contentType, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default);
    Task<Stream?> GetFileAsync(string relativePath, CancellationToken cancellationToken = default);
}
