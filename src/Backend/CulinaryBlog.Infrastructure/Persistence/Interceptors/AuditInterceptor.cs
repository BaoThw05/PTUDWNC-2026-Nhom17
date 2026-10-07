using CulinaryBlog.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CulinaryBlog.Infrastructure.Persistence.Interceptors;

public sealed class AuditInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateAuditFields(DbContext? context)
    {
        if (context is null) return;

        var utcNow = DateTimeOffset.UtcNow;

        foreach (EntityEntry<BaseEntity> entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = utcNow;
                }
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
            }
        }
    }
}
