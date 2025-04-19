using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KafkaPay.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KafkaPay.Shared.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        public DbSet<User> Users { get;}
        public DbSet<Account> Accounts { get; }
        public DbSet<TnxTransaction> TnxTransactions { get;  }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

}
