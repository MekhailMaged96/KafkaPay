using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KafkaPay.Shared.Domain.Common;
using KafkaPay.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace KafkaPay.Shared.Infrastructure.Data.Interceptors
{
    public  class AuditableEntityInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateEntities(eventData.Context);

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateEntities(eventData.Context);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public void UpdateEntities(DbContext? context)
        {
            if (context == null) return;

            var utcNow = DateTimeOffset.UtcNow;

            foreach (var entry in context.ChangeTracker.Entries())
            {
                var type = entry.Entity.GetType();

                if (entry.Entity is BaseAuditableEntity<Guid> entity &&
                    (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.HasChangedOwnedEntities()))
                {
                    if (entry.State == EntityState.Added)
                    {
                        entity.Created = utcNow;
                    }

                    entity.LastModified = utcNow;
                }
            }
        }

    }
    public static class Extensions
    {
        public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
            entry.References.Any(r =>
                r.TargetEntry != null &&
                r.TargetEntry.Metadata.IsOwned() &&
                (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
    }
}
