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
using KafkaPay.Shared.Application.Common.Exceptions.Behaviours;
using MediatR;

namespace KafkaPay.Shared.Infrastructure
{
    public static class DependencyInjection
    {
        public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("Default");

            builder.Services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            });

            builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
            builder.Services.AddScoped<ISaveChangesInterceptor, ConvertDomainEventsToOutBoxMessageInterceptor>();

            builder.Services.AddScoped(typeof(IKafkaProducer<>), typeof(KafkaProducer<>));
                

            builder.Services.AddDbContext<ApplicationDbContext>((sp, options) => {
                options.UseSqlServer(connectionString);
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            });

            builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());


        }
    }
}
