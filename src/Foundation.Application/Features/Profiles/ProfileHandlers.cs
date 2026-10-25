using Foundation.Application.Abstractions;
using Foundation.Application.Common;
using Foundation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Foundation.Application.Features.Profiles;

public class GetUserProfileQueryHandler : IQueryHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetUserProfileQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<UserProfileDto>> HandleAsync(GetUserProfileQuery query, CancellationToken cancellationToken = default)
    {
        var profile = await _db.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.UserId, cancellationToken);

        if (profile == null)
        {
            return Result.Failure<UserProfileDto>("User profile not found.");
        }

        bool isOwner = _currentUser.UserId.HasValue && _currentUser.UserId.Value == profile.Id;
        bool isAdmin = _currentUser.IsInRole("Admin");

        if (profile.IsPrivate && !isOwner && !isAdmin)
        {
            return Result.Failure<UserProfileDto>("This profile is private.");
        }

        var dto = new UserProfileDto(
            profile.Id,
            profile.DisplayName,
            profile.Bio,
            profile.AvatarUrl,
            profile.TimeZone,
            profile.IsPrivate,
            profile.CreatedAtUtc,
            profile.UpdatedAtUtc
        );

        return Result.Success(dto);
    }
}

public class UpdateUserProfileCommandHandler : ICommandHandler<UpdateUserProfileCommand, Result<UserProfileDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTime;

    public UpdateUserProfileCommandHandler(IAppDbContext db, ICurrentUser currentUser, IDateTimeProvider dateTime)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<Result<UserProfileDto>> HandleAsync(UpdateUserProfileCommand command, CancellationToken cancellationToken = default)
    {
        bool isOwner = _currentUser.UserId.HasValue && _currentUser.UserId.Value == command.UserId;
        bool isAdmin = _currentUser.IsInRole("Admin");

        if (!isOwner && !isAdmin)
        {
            return Result.Failure<UserProfileDto>("Forbidden: You are not authorized to update this profile.");
        }

        var profile = await _db.UserProfiles
            .FirstOrDefaultAsync(p => p.Id == command.UserId, cancellationToken);

        if (profile == null)
        {
            profile = new UserProfile
            {
                Id = command.UserId,
                DisplayName = command.DisplayName.Trim(),
                Bio = command.Bio?.Trim(),
                TimeZone = string.IsNullOrWhiteSpace(command.TimeZone) ? "UTC" : command.TimeZone.Trim(),
                IsPrivate = command.IsPrivate,
                AvatarUrl = command.AvatarUrl,
                CreatedAtUtc = _dateTime.UtcNow
            };
            _db.UserProfiles.Add(profile);
        }
        else
        {
            profile.DisplayName = command.DisplayName.Trim();
            profile.Bio = command.Bio?.Trim();
            profile.TimeZone = string.IsNullOrWhiteSpace(command.TimeZone) ? "UTC" : command.TimeZone.Trim();
            profile.IsPrivate = command.IsPrivate;
            if (command.AvatarUrl != null)
            {
                profile.AvatarUrl = command.AvatarUrl;
            }
            profile.UpdatedAtUtc = _dateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        var dto = new UserProfileDto(
            profile.Id,
            profile.DisplayName,
            profile.Bio,
            profile.AvatarUrl,
            profile.TimeZone,
            profile.IsPrivate,
            profile.CreatedAtUtc,
            profile.UpdatedAtUtc
        );

        return Result.Success(dto);
    }
}

public class ExportPersonalDataQueryHandler : IQueryHandler<ExportPersonalDataQuery, Result<PersonalDataExportDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTime;

    public ExportPersonalDataQueryHandler(IAppDbContext db, ICurrentUser currentUser, IDateTimeProvider dateTime)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<Result<PersonalDataExportDto>> HandleAsync(ExportPersonalDataQuery query, CancellationToken cancellationToken = default)
    {
        bool isOwner = _currentUser.UserId.HasValue && _currentUser.UserId.Value == query.UserId;
        bool isAdmin = _currentUser.IsInRole("Admin");

        if (!isOwner && !isAdmin)
        {
            return Result.Failure<PersonalDataExportDto>("Forbidden: You cannot export data for another user.");
        }

        var profile = await _db.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.UserId, cancellationToken);

        UserProfileDto? profileDto = profile != null
            ? new UserProfileDto(
                profile.Id,
                profile.DisplayName,
                profile.Bio,
                profile.AvatarUrl,
                profile.TimeZone,
                profile.IsPrivate,
                profile.CreatedAtUtc,
                profile.UpdatedAtUtc)
            : null;

        var pages = await _db.CmsPages
            .AsNoTracking()
            .Where(p => p.AuthorId == query.UserId)
            .Select(p => new PersonalContentSummaryDto(p.Id, p.Title, p.Slug, p.Status.ToString(), p.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        var posts = await _db.CmsPosts
            .AsNoTracking()
            .Where(p => p.AuthorId == query.UserId)
            .Select(p => new PersonalContentSummaryDto(p.Id, p.Title, p.Slug, p.Status.ToString(), p.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        string userIdString = query.UserId.ToString();
        var audit = await _db.AuditEntries
            .AsNoTracking()
            .Where(a => a.UserId == userIdString)
            .OrderByDescending(a => a.TimestampUtc)
            .Take(100)
            .Select(a => new PersonalAuditSummaryDto(a.EntityName, a.Action.ToString(), a.TimestampUtc))
            .ToListAsync(cancellationToken);

        var export = new PersonalDataExportDto(
            profileDto,
            pages,
            posts,
            audit,
            _dateTime.UtcNow
        );

        return Result.Success(export);
    }
}
