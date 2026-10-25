namespace Foundation.Application.Features.Profiles;

public record UserProfileDto(
    Guid Id,
    string DisplayName,
    string? Bio,
    string? AvatarUrl,
    string TimeZone,
    bool IsPrivate,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public record UpdateUserProfileCommand(
    Guid UserId,
    string DisplayName,
    string? Bio,
    string TimeZone,
    bool IsPrivate,
    string? AvatarUrl
);

public record GetUserProfileQuery(Guid UserId);

public record ExportPersonalDataQuery(Guid UserId);

public record PersonalDataExportDto(
    UserProfileDto? Profile,
    IReadOnlyList<PersonalContentSummaryDto> PagesAuthored,
    IReadOnlyList<PersonalContentSummaryDto> PostsAuthored,
    IReadOnlyList<PersonalAuditSummaryDto> AuditHistory,
    DateTime ExportedAtUtc
);

public record PersonalContentSummaryDto(
    Guid Id,
    string Title,
    string Slug,
    string Status,
    DateTime CreatedAtUtc
);

public record PersonalAuditSummaryDto(
    string EntityName,
    string Action,
    DateTime TimestampUtc
);
