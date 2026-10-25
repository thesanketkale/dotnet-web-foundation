using Foundation.Application.Abstractions;
using Foundation.Application.Common;
using Foundation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Foundation.Application.Features.Cms;

public class MediaHandlers :
    ICommandHandler<UploadMediaCommand, Result<MediaFileDto>>,
    ICommandHandler<DeleteMediaCommand, Result>,
    IQueryHandler<GetMediaFilesQuery, Result<PagedList<MediaFileDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IFileStorageService _storage;
    private readonly IDateTimeProvider _dateTime;

    public MediaHandlers(
        IAppDbContext db,
        ICurrentUser currentUser,
        IFileStorageService storage,
        IDateTimeProvider dateTime)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
        _dateTime = dateTime;
    }

    public async Task<Result<MediaFileDto>> HandleAsync(UploadMediaCommand command, CancellationToken cancellationToken = default)
    {
        if (command.FileStream == null || command.FileStream.Length == 0)
        {
            return Result.Failure<MediaFileDto>("Empty file stream.");
        }

        string relativePath;
        try
        {
            relativePath = await _storage.SaveFileAsync(
                command.FileStream,
                command.OriginalFileName,
                command.ContentType,
                cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<MediaFileDto>($"Failed to save file: {ex.Message}");
        }

        var storedFileName = Path.GetFileName(relativePath);
        var publicUrl = $"/api/v1/media/{storedFileName}";

        var media = new MediaFile
        {
            OriginalFileName = Path.GetFileName(command.OriginalFileName),
            StoredFileName = storedFileName,
            ContentType = command.ContentType,
            FileSizeBytes = command.FileSizeBytes,
            StoragePath = relativePath,
            PublicUrl = publicUrl,
            UploaderId = _currentUser.UserId ?? Guid.Empty,
            CreatedAtUtc = _dateTime.UtcNow
        };

        _db.MediaFiles.Add(media);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new MediaFileDto(
            media.Id,
            media.OriginalFileName,
            media.StoredFileName,
            media.ContentType,
            media.FileSizeBytes,
            media.PublicUrl,
            media.UploaderId,
            media.CreatedAtUtc
        );

        return Result.Success(dto);
    }

    public async Task<Result> HandleAsync(DeleteMediaCommand command, CancellationToken cancellationToken = default)
    {
        var media = await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken);
        if (media == null)
        {
            return Result.Failure("Media file not found.");
        }

        try
        {
            await _storage.DeleteFileAsync(media.StoragePath, cancellationToken);
        }
        catch
        {
            // Suppress storage delete error if file was already unlinked
        }

        _db.MediaFiles.Remove(media);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<PagedList<MediaFileDto>>> HandleAsync(GetMediaFilesQuery query, CancellationToken cancellationToken = default)
    {
        var queryable = _db.MediaFiles.AsNoTracking();
        var total = await queryable.CountAsync(cancellationToken);

        var items = await queryable
            .OrderByDescending(m => m.CreatedAtUtc)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(m => new MediaFileDto(
                m.Id,
                m.OriginalFileName,
                m.StoredFileName,
                m.ContentType,
                m.FileSizeBytes,
                m.PublicUrl,
                m.UploaderId,
                m.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedList<MediaFileDto>(items, total, query.PageNumber, query.PageSize));
    }
}
