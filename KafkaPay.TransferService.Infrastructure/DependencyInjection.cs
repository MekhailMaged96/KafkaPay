using KafkaPay.Shared.Application.Common.Exceptions.Behaviours;
using KafkaPay.Shared.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KafkaPay.TransferService.Infrastructure
{
    public static class DependencyInjection
    {
        public static void AddTransferInfrastructureServices(this IHostApplicationBuilder builder)
        {


            builder.Services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            });
            builder.Services.AddHostedService<TransactionEventConsumer>();


        }
    }
}
