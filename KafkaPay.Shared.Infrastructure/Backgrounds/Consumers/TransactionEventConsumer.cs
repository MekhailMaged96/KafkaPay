using System.Threading;
using Confluent.Kafka;
using KafkaPay.Shared.Application.Common.Interfaces;
using KafkaPay.Shared.Domain.Constants;
using KafkaPay.Shared.Domain.Entities;
using KafkaPay.Shared.Domain.Events;
using KafkaPay.TransferService.Application.Features.Commands.CompleteTransfer;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
    protected override  Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Factory.StartNew(async ()  =>
        {
            using var consumerScope = _serviceScopeFactory.CreateScope();
            var kafkaConsumer = consumerScope.ServiceProvider.GetRequiredService<IKafkaConsumer<Ignore, TransactionInitiatedEvent>>();
            var logger = consumerScope.ServiceProvider.GetRequiredService<ILogger<TransactionEventConsumer>>();

            kafkaConsumer.Subscribe(_topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = kafkaConsumer.Consume(stoppingToken);
                        if (consumeResult?.Message == null)
                            continue;

                        logger.LogInformation("Consumed message from {Offset}", consumeResult.TopicPartitionOffset);

                        using var scope = _serviceScopeFactory.CreateScope();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

                        var message = consumeResult.Message.Value;
                        var consumerName = nameof(TransactionEventConsumer);

                        var alreadyHandled = await dbContext.OutboxMessageConsumers
                            .AnyAsync(x => x.Id == message.TransactionId && x.Name == consumerName, stoppingToken);

                        if (alreadyHandled)
                        {
                            logger.LogInformation("Message already processed. Skipping.");
                            continue;
                        }

                        await mediator.Send(new CompleteTransferCommand(message.TransactionId), stoppingToken);

                        dbContext.OutboxMessageConsumers.Add(new OutboxMessageConsumer
                        {
                            Id = message.TransactionId,
                            Name = consumerName
                        });

                        await dbContext.SaveChangesAsync(stoppingToken);

                        kafkaConsumer.Commit(consumeResult);

                    }
                    catch (ConsumeException ex)
                    {
                        logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Unhandled error in Kafka consumer loop.");
                        await Task.Delay(1000, stoppingToken); // backoff on error
                    }
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Kafka consumer is shutting down.");
            }
            finally
            {
                kafkaConsumer.Close();
                logger.LogInformation("Kafka consumer closed.");
            }
        }); 

      
    }

}
