using System.Text.Json;
using Foundation.Application.Abstractions;
using Foundation.Domain.Common;
using Foundation.Domain.Entities;
using Foundation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Foundation.Infrastructure.Persistence.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTime;

    public AuditInterceptor(ICurrentUser currentUser, IDateTimeProvider dateTime)
    {
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        UpdateAuditableEntities(eventData.Context);
        GenerateAuditEntries(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditableEntities(DbContext context)
    {
        var utcNow = _dateTime.UtcNow;
        var userId = _currentUser.UserName ?? _currentUser.UserId?.ToString();

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = utcNow;
                entry.Entity.CreatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = utcNow;
                entry.Entity.UpdatedBy = userId;
            }
        }
    }

    private void GenerateAuditEntries(DbContext context)
    {
        var utcNow = _dateTime.UtcNow;
        var userId = _currentUser.UserId?.ToString();
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            // Do not audit the AuditEntry entity itself
            if (entry.Entity is AuditEntry || entry.State is EntityState.Detached or EntityState.Unchanged)
            {
                continue;
            }

            var entityName = entry.Entity.GetType().Name;
            var primaryKey = GetPrimaryKeyValue(entry);

            var action = entry.State switch
            {
                EntityState.Added => AuditAction.Create,
                EntityState.Modified => AuditAction.Update,
                EntityState.Deleted => AuditAction.Delete,
                _ => AuditAction.Update
            };

            var changes = new Dictionary<string, object?>();

            if (entry.State == EntityState.Added)
            {
                foreach (var prop in entry.Properties)
                {
                    if (!prop.Metadata.IsPrimaryKey() && prop.CurrentValue != null)
                    {
                        changes[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }
            }
            else if (entry.State == EntityState.Deleted)
            {
                foreach (var prop in entry.Properties)
                {
                    if (!prop.Metadata.IsPrimaryKey() && prop.OriginalValue != null)
                    {
                        changes[prop.Metadata.Name] = prop.OriginalValue;
                    }
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                foreach (var prop in entry.Properties)
                {
                    if (prop.IsModified && !Equals(prop.OriginalValue, prop.CurrentValue))
                    {
                        changes[prop.Metadata.Name] = new
                        {
                            Old = prop.OriginalValue,
                            New = prop.CurrentValue
                        };
                    }
                }
            }

            if (changes.Count > 0 || action != AuditAction.Update)
            {
                var audit = new AuditEntry
                {
                    UserId = userId,
                    EntityName = entityName,
                    EntityId = primaryKey,
                    Action = action,
                    TimestampUtc = utcNow,
                    ChangesJson = JsonSerializer.Serialize(changes)
                };

                auditEntries.Add(audit);
            }
        }

        if (auditEntries.Count > 0)
        {
            context.Set<AuditEntry>().AddRange(auditEntries);
        }
    }

    private static string GetPrimaryKeyValue(EntityEntry entry)
    {
        var keyProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
        return keyProperty?.CurrentValue?.ToString() ?? Guid.Empty.ToString();
    }
}
