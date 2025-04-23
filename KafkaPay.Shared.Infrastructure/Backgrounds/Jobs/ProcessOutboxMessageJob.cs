using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Confluent.Kafka;
using KafkaPay.Shared.Application.Common.Interfaces;
using KafkaPay.Shared.Domain.Constants;
using KafkaPay.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;

namespace KafkaPay.Shared.Infrastructure.Backgrounds.Jobs
{
    public class ProcessOutboxMessageJob
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IKafkaProducer<object> _kafkaProducer;
        private readonly string _topic;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly ILogger<ProcessOutboxMessageJob> _logger;

        public ProcessOutboxMessageJob(
            IApplicationDbContext dbContext,
            IKafkaProducer<object> kafkaProducer,
            IConfiguration configuration,
            ILogger<ProcessOutboxMessageJob> logger)
        {
            _dbContext = dbContext;
            _kafkaProducer = kafkaProducer;
            _topic = configuration["KafkaSettings:TransactionTopic"] ?? KafkaTopics.TransactionTopic;
            _logger = logger;

            _retryPolicy = Policy
                .Handle<ProduceException<Null, object>>() // Retry on Kafka produce exceptions
                .Or<Exception>(ex => IsTransientError(ex)) // Extend to other transient errors
                .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, "[RetryPolicy] Retry {RetryCount} after {Delay}s", retryCount, timeSpan.TotalSeconds);
                    });
        }

        public async Task Execute()
        {
            var outboxMessages = await GetPendingOutboxMessages();

            foreach (var message in outboxMessages)
            {
                try
                {
                    await _retryPolicy.ExecuteAsync(() => ProcessMessageAsync(message));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process message {MessageId} after all retries", message.Id);
                }
            }
        }

        private async Task<List<OutBoxMessage>> GetPendingOutboxMessages()
        {
            return await _dbContext.OutBoxMessages
                .Where(m => m.ProcessedOnUtc == null && m.Error == null)
                .Take(10)
                .ToListAsync();
        }

        private async Task ProcessMessageAsync(OutBoxMessage message)
        {
            using var transaction = await _dbContext.BeginTransactionAsync();

            try
            {
                // Deserialize the message content
                var messageType = Assembly.Load("KafkaPay.Shared.Domain").GetType(message.Type);
                if (messageType == null)
                {
                    throw new InvalidOperationException($"Type {message.Type} not found.");
                }

                var domainEvent = JsonConvert.DeserializeObject(message.Content, messageType);
                if (domainEvent == null)
                {
                    throw new InvalidOperationException("Failed to deserialize message content.");
                }
                message.MarkAsProcessed(DateTime.UtcNow);
                await _dbContext.SaveChangesAsync();

                // Produce to Kafka (retries handled by policy)
                await _kafkaProducer.ProduceAsync(_topic, domainEvent);

                // Mark as processed only after successful Kafka production
      
                await transaction.CommitAsync();
            }
            catch (Exception ex) when (IsNonRetriableError(ex))
            {
                // Handle non-retriable errors (e.g., deserialization)
                await transaction.RollbackAsync();
                await MarkMessageAsFailedAsync(message, ex);
                _logger.LogError(ex, "Non-retriable error processing message {MessageId}", message.Id);
            }
            catch (Exception ex)
            {
                // Retriable errors will be retried by the policy; log and rethrow
                await transaction.RollbackAsync();
                _logger.LogWarning(ex, "Retriable error processing message {MessageId}", message.Id);
                throw;
            }
        }

        private async Task MarkMessageAsFailedAsync(OutBoxMessage message, Exception ex)
        {
            using var transaction = await _dbContext.BeginTransactionAsync();
            try
            {
                message.MarkAsFailed(ex.Message);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception innerEx)
            {
                _logger.LogCritical(innerEx, "Failed to mark message {MessageId} as failed", message.Id);
                await transaction.RollbackAsync();
                // Consider additional handling for critical failures
            }
        }

        private bool IsTransientError(Exception ex)
        {
            // Example: Network issues, Kafka timeouts, etc.
            return ex is KafkaException || ex is DbUpdateConcurrencyException;
        }

        private bool IsNonRetriableError(Exception ex)
        {
            // Example: Deserialization errors, unknown types
            return ex is JsonSerializationException || ex is InvalidOperationException;
        }
    }
}