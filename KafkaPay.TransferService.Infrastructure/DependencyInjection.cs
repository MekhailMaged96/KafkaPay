using Confluent.Kafka;
using KafkaPay.Shared.Application.Common.Exceptions.Behaviours;
using KafkaPay.TransferService.Application.Handlers;
using KafkaPay.TransferService.Infrastructure.Backgrounds;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace KafkaPay.TransferService.Infrastructure
{
    public static class DependencyInjection
    {
        public static void AddTransferInfrastructureServices(this IHostApplicationBuilder builder)
        {

            var configuration = builder.Services.BuildServiceProvider().GetRequiredService<IConfiguration>();

            builder.Services.AddTransient<TelemetryHandler>();

            builder.Services.AddHttpClient("kafka-client").AddHttpMessageHandler<TelemetryHandler>();


            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            });

            var bootstrapServers = configuration["KafkaSettings:BootstrapServers"];
            var groupId = configuration["KafkaSettings:GroupId"];
            var autoOffsetReset = Enum.TryParse(configuration["KafkaSettings:AutoOffsetReset"], out AutoOffsetReset reset)
                                  ? reset
                                  : AutoOffsetReset.Earliest;
            var enableAutoCommit = bool.TryParse(configuration["KafkaSettings:EnableAutoCommit"], out bool autoCommit)
                                   ? autoCommit
                                   : false;

            builder.Services.AddSingleton(new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = autoOffsetReset,
                EnableAutoCommit = enableAutoCommit
            });


           
            builder.Services.AddSingleton<TransactionEventConsumer>();
            builder.Services.AddHostedService<TransactionKafkaHostedService>();

        }
    }
}
