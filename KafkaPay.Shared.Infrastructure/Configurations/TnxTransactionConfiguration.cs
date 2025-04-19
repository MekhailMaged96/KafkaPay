using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KafkaPay.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KafkaPay.Shared.Infrastructure.Configurations
{
    public class TnxTransactionConfiguration : IEntityTypeConfiguration<TnxTransaction>
    {
        public void Configure(EntityTypeBuilder<TnxTransaction> builder)
        {
           
            builder.HasOne(e => e.FromAccount)
                   .WithMany()
                   .HasForeignKey(e => e.FromAccountId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.ToAccount)
                   .WithMany()
                   .HasForeignKey(e => e.ToAccountId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
