using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KafkaPay.Shared.Application.Common.Interfaces;
using KafkaPay.Shared.Domain.Common;
using KafkaPay.Shared.Domain.Entities;
using KafkaPay.Shared.Domain.Enums;
using KafkaPay.Shared.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace KafkaPay.Shared.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new TnxTransactionConfiguration());
            modelBuilder.Entity<TransactionStatus>().HasData(
                      new TransactionStatus { Id = (int)TnxTransactionStatusEnum.Pending, Name = "Pending" },
                      new TransactionStatus { Id = (int)TnxTransactionStatusEnum.Completed, Name = "Completed" },
                      new TransactionStatus { Id = (int)TnxTransactionStatusEnum.Failed, Name = "Failed" },
                      new TransactionStatus { Id = (int)TnxTransactionStatusEnum.Cancelled, Name = "Cancelled" }
                  );
        }
      
     
        public DbSet<User> Users => Set<User>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<TnxTransaction> TnxTransactions => Set<TnxTransaction>();
        public DbSet<TransactionStatus> TransactionStatuses => Set<TransactionStatus>();
        public DbSet<OutBoxMessage> OutBoxMessages => Set<OutBoxMessage>();
        public DbSet<OutboxMessageConsumer> OutboxMessageConsumers => Set<OutboxMessageConsumer>();

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
           
            return await Database.BeginTransactionAsync();
        }

    }
}
