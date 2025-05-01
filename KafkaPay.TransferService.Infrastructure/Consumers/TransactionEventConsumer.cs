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
using Serilog.Core;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;
using System.Diagnostics;
using System.Text;

public class TransactionEventConsumer : KafkaBaseConsumer<Ignore, TransactionInitiatedEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly string _topic;
    private const string ConsumerName = nameof(TransactionEventConsumer);
    private static readonly ActivitySource ActivitySource = new("Kafka.Produce");

    public TransactionEventConsumer(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, ConsumerConfig consumerConfig, ILogger<TransactionEventConsumer> logger)
        : base(consumerConfig, logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _topic = _configuration["KafkaSettings:TransactionTopic"] ?? KafkaTopics.TransactionTopic;
        logger.LogInformation("TransactionEventConsumer initialized for topic: {Topic}", _topic);

    }

    // This method runs as part of background service lifecycle
    public async Task ConsumeMessagesAsync(CancellationToken stoppingToken)
    {
        using var consumerScope = _serviceScopeFactory.CreateScope();
        var logger = consumerScope.ServiceProvider.GetRequiredService<ILogger<TransactionEventConsumer>>();

        Subscribe(_topic);
        logger.LogInformation("Subscribed to Kafka topic: {Topic}", _topic);

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

            logger.LogInformation("Committed offset for Transaction ID: {TransactionId}", consumeResult.Message.Value.TransactionId);

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

        var parentContext = Propagators.DefaultTextMapPropagator.Extract(
             default,
           consumeResult.Message.Headers,
           (headers, key) =>
           {
               var header = headers.LastOrDefault(h => h.Key == key);
               return header?.GetValueBytes() is byte[] value ? new[] { Encoding.UTF8.GetString(value) } : Array.Empty<string>();
           });

        Baggage.Current = parentContext.Baggage;

        using var activity = ActivitySource.StartActivity("ConsumeKafkaMessage", ActivityKind.Consumer, parentContext.ActivityContext);

        activity?.SetTag("messaging.kafka.topic", consumeResult.Topic);
        activity?.SetTag("messaging.kafka.partition", consumeResult.Partition.Value);
        activity?.SetTag("messaging.kafka.offset", consumeResult.Offset.Value);


        if (await IsMessageAlreadyProcessedAsync(dbContext, transactionId, stoppingToken))
        {
            logger.LogInformation("Transaction {TransactionId} already processed", transactionId);
            return;
        }

        await ProcessTransactionAsync(mediator, transactionId, stoppingToken);
        logger.LogInformation("Transaction {TransactionId} processed successfully", transactionId);

        await RecordProcessedMessageAsync(dbContext, transactionId, stoppingToken);

        logger.LogInformation("Transaction {TransactionId} recorded as processed", transactionId);

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
        using var processingScope = _serviceScopeFactory.CreateScope();
        var services = processingScope.ServiceProvider;

        var httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient();

        var url = $"https://localhost:7163/api/transfer/complete/{transactionId}";
        try
        {
            var response = await httpClient.PostAsync(url, null, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Transaction {TransactionId} completed successfully via HTTP.", transactionId);
            }
            else
            {
                _logger.LogWarning("Failed to complete transaction {TransactionId}. Status code: {StatusCode}",
                    transactionId, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP request failed for transaction {TransactionId}", transactionId);
            throw;
        }


        //await mediator.Send(new CompleteTransferCommand(transactionId), cancellationToken);
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
