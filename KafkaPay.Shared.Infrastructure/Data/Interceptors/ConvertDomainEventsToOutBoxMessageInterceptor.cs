using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KafkaPay.Shared.Domain.Common;
using KafkaPay.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Polly;

namespace KafkaPay.Shared.Infrastructure.Data.Interceptors
{
    public class ConvertDomainEventsToOutBoxMessageInterceptor  : SaveChangesInterceptor
    {

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, 
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            DbContext dbContext = eventData?.Context;

            if(dbContext is null)
            {
                return base.SavingChangesAsync(eventData, result, cancellationToken);
            }

          var domainEvents = dbContext.ChangeTracker
                            .Entries<BaseEntity<Guid>>()
                            .Where(entry => entry.Entity.DomainEvents?.Any() == true)
                            .SelectMany(entry =>
                            {
                                var events = entry.Entity.DomainEvents.ToList();
                                entry.Entity.ClearDomainEvents();
                                return events;
                            })
                            .ToList();


            var outBoxMessages = domainEvents
                                .Select(domainEvent => new OutBoxMessage(
                                    domainEvent.GetType().FullName!,
                                    domainEvent,
                                    DateTime.UtcNow
                                ))
                                .ToList();

            if (outBoxMessages.Any())
            {
                dbContext.Set<OutBoxMessage>().AddRange(outBoxMessages);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
