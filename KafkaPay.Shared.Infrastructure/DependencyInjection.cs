using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using KafkaPay.Shared.Application.Common.Interfaces;
using KafkaPay.Shared.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using KafkaPay.Shared.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore.Diagnostics;
using KafkaPay.Shared.Infrastructure.MessageBrokers;

namespace KafkaPay.Shared.Infrastructure
{
    public static class DependencyInjection
    {
        public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("Default");

            builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

            builder.Services.AddScoped<IKafkaConsumer, KafkaConsumer>();
            builder.Services.AddScoped<IKafkaProducer, KafkaProducer>();

            builder.Services.AddDbContext<ApplicationDbContext>((sp, options) => {
                var interceptor = sp.GetRequiredService<ISaveChangesInterceptor>();
                options.UseSqlServer(connectionString);
                options.AddInterceptors(interceptor);

            });

            builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());


        }
    }
}
