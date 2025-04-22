using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using KafkaPay.Shared.Application.Common.Interfaces;
using KafkaPay.Shared.Domain.Constants;
using KafkaPay.Shared.Domain.Entities;
using KafkaPay.Shared.Infrastructure.MessageBrokers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;

namespace KafkaPay.Shared.Infrastructure.Backgrounds.Jobs
{
    public class ProcessOutboxMessageJob
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IKafkaProducer<object> _kafkaProducer;
        private readonly IConfiguration _configuration;
        private readonly string _topic;
        private readonly AsyncRetryPolicy _retryPolicy;
        public ProcessOutboxMessageJob(IApplicationDbContext dbContext, IKafkaProducer<object> kafkaProducer, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _kafkaProducer = kafkaProducer;
            _configuration = configuration;
            _topic = _configuration["KafkaSettings:TransactionTopic"] ?? KafkaTopics.TransactionTopic;
            _retryPolicy = Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(3, retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        onRetry: (exception, timeSpan, retryCount, context) =>
                        {
                            Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to: {exception.Message}");
                        });
        }

        public async Task Execute()
        {
            var outboxMessages = await GetPendingOutboxMessages();

            if (outboxMessages.Any())
            {
                foreach (var message in outboxMessages)
                {
                    await _retryPolicy.ExecuteAsync(() => ProcessMessageAsync(message));

                }

            }
        }
        private async Task<List<OutBoxMessage>> GetPendingOutboxMessages()
        {
            return await _dbContext.OutBoxMessages
                                                  .Where(m => m.ProcessedOnUtc == null)
                                                  .Take(10)
                                                  .ToListAsync();
        }

        private async Task ProcessMessageAsync(OutBoxMessage message)
        {
            using var transaction = await _dbContext.BeginTransactionAsync();

            try
            {
                var messageType = Assembly.Load("KafkaPay.Shared.Domain").GetType(message.Type);
                var domainEvent = messageType != null ? JsonConvert.DeserializeObject(message.Content, messageType) : null;

                if (domainEvent != null)
                {
                    message.MarkAsProcessed(DateTime.UtcNow);
                    await _dbContext.SaveChangesAsync();
                    await _kafkaProducer.ProduceAsync(_topic, domainEvent);
                    await transaction.CommitAsync();
                }
                else
                {
                    message.MarkAsFailed("Failed to deserialize message");
                    await _dbContext.SaveChangesAsync();
                    await transaction.RollbackAsync();
                }
            }
            catch (Exception ex)
            {
                message.MarkAsFailed(ex.Message);
                await _dbContext.SaveChangesAsync();
                await transaction.RollbackAsync();

                throw;
            }
        }
    }
}


