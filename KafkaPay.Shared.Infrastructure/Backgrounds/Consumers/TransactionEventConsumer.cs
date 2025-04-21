using Confluent.Kafka;
using KafkaPay.Shared.Application.Common.Interfaces;
using KafkaPay.Shared.Domain.Constants;
using KafkaPay.Shared.Domain.Events;
using KafkaPay.TransferService.Application.Features.Commands.CompleteTransfer;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class TransactionEventConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly string _topic = KafkaTopics.TransactionTopic;

    public TransactionEventConsumer(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var kafkaConsumer = scope.ServiceProvider.GetRequiredService<IKafkaConsumer<Ignore, TransactionInitiatedEvent>>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TransactionEventConsumer>>();

        kafkaConsumer.Subscribe(_topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = kafkaConsumer.Consume(stoppingToken);

                    if (consumeResult?.Message == null) continue;

                    logger.LogInformation("Consumed message from {TopicPartitionOffset}", consumeResult.TopicPartitionOffset);

                    var message = consumeResult.Message.Value;
                    var command = new CompleteTransferCommand(message.TransactionId);

                    await mediator.Send(command, stoppingToken);
                }
                catch (ConsumeException e)
                {
                    logger.LogError($"Error while consuming message: {e.Error.Reason}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Kafka Consumer was cancelled.");
        }
        finally
        {
            kafkaConsumer.Close();
        }
    }
}
