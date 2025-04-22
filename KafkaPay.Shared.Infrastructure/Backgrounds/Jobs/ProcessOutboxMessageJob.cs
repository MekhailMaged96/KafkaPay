using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using KafkaPay.Shared.Application.Common.Interfaces;
using KafkaPay.Shared.Domain.Constants;
using KafkaPay.Shared.Infrastructure.MessageBrokers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KafkaPay.Shared.Infrastructure.Backgrounds.Jobs
{
    public class ProcessOutboxMessageJob
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IKafkaProducer<object> _kafkaProducer;

        public ProcessOutboxMessageJob(IApplicationDbContext dbContext,IKafkaProducer<object> kafkaProducer)
        {
            _dbContext = dbContext;
           _kafkaProducer = kafkaProducer;
        }

        public async Task Execute()
        {
            var outboxMessages = await _dbContext.OutBoxMessages
                                                  .Where(m=>m.ProcessedOnUtc == null)
                                                  .Take(10)
                                                  .ToListAsync();


            if (outboxMessages.Any())
            {
                foreach (var message in outboxMessages)
                {
                     using var transaction = await _dbContext.BeginTransactionAsync();

                    try
                    {
                        var assembly = Assembly.Load("KafkaPay.Shared.Domain");

                        var messageType = assembly.GetType(message.Type);


                        if (messageType == null)
                        {
                            continue;
                        }

                        var domainEvent = JsonConvert.DeserializeObject(message.Content, messageType);

                        if (domainEvent == null)
                        {
                            // Optionally log: deserialization failed
                            continue;
                        }
                        message.MarkAsProcessed(DateTime.UtcNow);

                        await _dbContext.SaveChangesAsync();

                        await _kafkaProducer.ProduceAsync(KafkaTopics.TransactionTopic, domainEvent);


                  
                        await transaction.CommitAsync(); // <-- COMMIT

                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        message.MarkAsFailed(ex.Message);
                        await _dbContext.SaveChangesAsync();
                        // Optionally log the error
                    }
                    finally
                    {
                        _dbContext.OutBoxMessages.Update(message);
                    }
                }

            }


        }
    }

}
