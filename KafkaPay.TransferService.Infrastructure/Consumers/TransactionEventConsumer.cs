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
    private const string ConsumerName = nameof(TransactionEventConsumer);

    public TransactionEventConsumer(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () => await ConsumeMessagesAsync(stoppingToken), stoppingToken);
    }

    private async Task ConsumeMessagesAsync(CancellationToken stoppingToken)
    {
        using var consumerScope = _serviceScopeFactory.CreateScope();
        var kafkaConsumer = consumerScope.ServiceProvider.GetRequiredService<IKafkaConsumer<Ignore, TransactionInitiatedEvent>>();
        var logger = consumerScope.ServiceProvider.GetRequiredService<ILogger<TransactionEventConsumer>>();

        kafkaConsumer.Subscribe(_topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessNextMessageAsync(kafkaConsumer, logger, stoppingToken);
            }
        }
        finally
        {
            kafkaConsumer.Close();
            logger.LogInformation("Kafka consumer closed");
        }
    }

    private async Task ProcessNextMessageAsync(
        IKafkaConsumer<Ignore, TransactionInitiatedEvent> kafkaConsumer,
        ILogger<TransactionEventConsumer> logger,
        CancellationToken stoppingToken)
    {
        try
        {
            var consumeResult = kafkaConsumer.Consume(stoppingToken);
            if (consumeResult?.Message?.Value == null) return;

            logger.LogInformation("Consumed message for Transaction ID: {TransactionId} from {Offset}",
                consumeResult.Message.Value.TransactionId,
                consumeResult.TopicPartitionOffset);

            await ProcessMessageWithScopedServicesAsync(consumeResult, stoppingToken);

            kafkaConsumer.Commit(consumeResult);
        }
        catch (ConsumeException ex)
        {
            logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Kafka message");
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessMessageWithScopedServicesAsync(
        ConsumeResult<Ignore, TransactionInitiatedEvent> consumeResult,
        CancellationToken stoppingToken)
    {
        using var processingScope = _serviceScopeFactory.CreateScope();
        var services = processingScope.ServiceProvider;

        var mediator = services.GetRequiredService<IMediator>();
        var dbContext = services.GetRequiredService<IApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<TransactionEventConsumer>>();

        var message = consumeResult.Message.Value;
        var transactionId = message.TransactionId;


        if (await IsMessageAlreadyProcessedAsync(dbContext, transactionId, stoppingToken))
        {
            logger.LogInformation("Transaction {TransactionId} already processed", transactionId);
            return;
        }

        await ProcessTransactionAsync(mediator, transactionId, stoppingToken);
        await RecordProcessedMessageAsync(dbContext, transactionId, stoppingToken);
    }

    private async Task<bool> IsMessageAlreadyProcessedAsync(
        IApplicationDbContext dbContext,
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        return await dbContext.OutboxMessageConsumers
            .AnyAsync(x => x.Id == transactionId && x.Name == ConsumerName, cancellationToken);
    }

    private async Task ProcessTransactionAsync(
        IMediator mediator,
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new CompleteTransferCommand(transactionId), cancellationToken);
    }

    private async Task RecordProcessedMessageAsync(
        IApplicationDbContext dbContext,
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        dbContext.OutboxMessageConsumers.Add(new OutboxMessageConsumer
        {
            Id = transactionId,
            Name = ConsumerName
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}