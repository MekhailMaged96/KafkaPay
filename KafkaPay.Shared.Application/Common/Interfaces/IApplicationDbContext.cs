using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KafkaPay.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace KafkaPay.Shared.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        public DbSet<User> Users { get;}
        public DbSet<Account> Accounts { get; }
        public DbSet<TnxTransaction> TnxTransactions { get;  }
        public DbSet<OutBoxMessage> OutBoxMessages { get; }
        public DbSet<OutboxMessageConsumer> OutboxMessageConsumers { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }

}
