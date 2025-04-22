using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using KafkaPay.Shared.Application.Common.Interfaces;
using KafkaPay.Shared.Domain.Constants;
using KafkaPay.Shared.Domain.Entities;
using KafkaPay.Shared.Domain.Events;
using KafkaPay.Shared.Infrastructure.Consumers;
using KafkaPay.TransferService.Application.Features.Commands.CompleteTransfer;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class TransactionEventConsumer : KafkaBaseConsumer<Ignore, TransactionInitiatedEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly string _topic;
    private const string ConsumerName = nameof(TransactionEventConsumer);

    public TransactionEventConsumer(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, ConsumerConfig consumerConfig, ILogger<TransactionEventConsumer> logger)
        : base(consumerConfig, logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _topic = _configuration["KafkaSettings:TransactionTopic"] ?? KafkaTopics.TransactionTopic;
    }

    // This method runs as part of background service lifecycle
    public async Task ConsumeMessagesAsync(CancellationToken stoppingToken)
    {
        using var consumerScope = _serviceScopeFactory.CreateScope();
        var logger = consumerScope.ServiceProvider.GetRequiredService<ILogger<TransactionEventConsumer>>();

        Subscribe(_topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessNextMessageAsync(logger, stoppingToken);
            }
        }
        finally
        {
            Close();
            logger.LogInformation("Kafka consumer closed");
        }
    }

    private async Task ProcessNextMessageAsync(
        ILogger<TransactionEventConsumer> logger,
        CancellationToken stoppingToken)
    {
        try
        {
            var consumeResult = Consume(stoppingToken);
            if (consumeResult?.Message?.Value == null) return;

            logger.LogInformation("Consumed message for Transaction ID: {TransactionId} from {Offset}",
                consumeResult.Message.Value.TransactionId,
                consumeResult.TopicPartitionOffset);

            await ProcessMessageWithScopedServicesAsync(consumeResult, stoppingToken);

           Commit(consumeResult);
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
