using Confluent.Kafka;
using KafkaPay.Shared.Application.Common.Exceptions.Behaviours;
using KafkaPay.Shared.Application.Common.Interfaces;
using KafkaPay.Shared.Infrastructure.MessageBrokers;
using KafkaPay.TransferService.Infrastructure.Backgrounds;
using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KafkaPay.TransferService.Infrastructure
{
    public static class DependencyInjection
    {
        public static void AddTransferInfrastructureServices(this IHostApplicationBuilder builder)
        {


            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            });
            builder.Services.AddSingleton(new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "kafka-consumer-id1",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            }
            );
            builder.Services.AddSingleton<TransactionEventConsumer>();
            builder.Services.AddHostedService<TransactionKafkaHostedService>();

        }
    }
}
