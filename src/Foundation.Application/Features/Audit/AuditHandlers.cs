using Foundation.Application.Abstractions;
using Foundation.Application.Common;
using Foundation.Domain.Entities;
using Foundation.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Foundation.Application.Features.Audit;

public record AuditLogDto(
    Guid Id,
    string? UserId,
    string EntityName,
    string EntityId,
    AuditAction Action,
    DateTime TimestampUtc,
    string ChangesJson,
    string? IpAddress
);

public record GetAuditLogsQuery(
    int PageNumber = 1,
    int PageSize = 25,
    string? EntityName = null,
    string? UserId = null
);

public class GetAuditLogsQueryHandler : IQueryHandler<GetAuditLogsQuery, Result<PagedList<AuditLogDto>>>
{
    private readonly IAppDbContext _db;

    public GetAuditLogsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<PagedList<AuditLogDto>>> HandleAsync(GetAuditLogsQuery query, CancellationToken cancellationToken = default)
    {
        var queryable = _db.AuditEntries.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.EntityName))
        {
            queryable = queryable.Where(a => a.EntityName == query.EntityName);
        }

        if (!string.IsNullOrWhiteSpace(query.UserId))
        {
            queryable = queryable.Where(a => a.UserId == query.UserId);
        }

        var total = await queryable.CountAsync(cancellationToken);

        var items = await queryable
            .OrderByDescending(a => a.TimestampUtc)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(a => new AuditLogDto(
                a.Id,
                a.UserId,
                a.EntityName,
                a.EntityId,
                a.Action,
                a.TimestampUtc,
                a.ChangesJson,
                a.IpAddress
            ))
            .ToListAsync(cancellationToken);

        return Result<PagedList<AuditLogDto>>.Success(new PagedList<AuditLogDto>(items, total, query.PageNumber, query.PageSize));
    }
}
