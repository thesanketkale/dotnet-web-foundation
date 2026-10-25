using System.Text;
using Foundation.Infrastructure.Services;
using Shouldly;
using Xunit;

namespace Foundation.UnitTests;

public class StorageSecurityTests : IDisposable
{
    private readonly string _testDirectory;

    public StorageSecurityTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "StorageTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task SaveFile_Should_Reject_Disallowed_Extensions()
    {
        var service = new LocalFileStorageService(_testDirectory);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("malicious content"));

        await Should.ThrowAsync<InvalidOperationException>(async () =>
        {
            await service.SaveFileAsync(stream, "script.exe", "application/octet-stream");
        });
    }

    [Fact]
    public async Task SaveFile_Should_Store_Valid_Image_With_Unique_Name()
    {
        var service = new LocalFileStorageService(_testDirectory);
        // Valid PNG header magic bytes
        byte[] pngHeader = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00];
        using var stream = new MemoryStream(pngHeader);

        var storedName = await service.SaveFileAsync(stream, "photo.png", "image/png");

        storedName.ShouldEndWith(".png");
        storedName.ShouldNotBe("photo.png"); // Name must be randomized/UUID to avoid path collision

        var filePath = Path.Combine(_testDirectory, storedName);
        File.Exists(filePath).ShouldBeTrue();
    }

    [Fact]
    public async Task DeleteFile_Should_Safely_Remove_Existing_File()
    {
        var service = new LocalFileStorageService(_testDirectory);
        byte[] pngHeader = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00];
        using var stream = new MemoryStream(pngHeader);

        var storedName = await service.SaveFileAsync(stream, "avatar.png", "image/png");
        await service.DeleteFileAsync(storedName);

        var filePath = Path.Combine(_testDirectory, storedName);
        File.Exists(filePath).ShouldBeFalse();
    }
}
