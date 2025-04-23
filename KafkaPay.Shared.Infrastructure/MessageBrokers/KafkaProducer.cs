using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using KafkaPay.Shared.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KafkaPay.Shared.Infrastructure.MessageBrokers
{
    public class KafkaProducer<T> : IKafkaProducer<T>
    {
        private readonly IProducer<Null, string> _producer;
        private readonly ILogger<KafkaProducer<T>> _logger;

        public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer<T>> logger)
        {
            _logger = logger;

            var config = new ProducerConfig
            {
                BootstrapServers = configuration["KafkaSettings:BootstrapServers"],
                EnableIdempotence = true,
                MessageTimeoutMs = 5000, // Fail after 5 seconds
                RequestTimeoutMs = 3000, // Shorter timeout for connection attempts
            };

            _producer = new ProducerBuilder<Null, string>(config)
                .SetErrorHandler((producer, error) =>
                {
                    _logger.LogError(
                        "Kafka producer error: {Code} - {Message}",
                        error.Code,
                        error.Reason
                    );
                })
                .Build();
        }

        public async Task ProduceAsync(string topic, T message)
        {
            try
            {
                // Force a connection check by fetching metadata

                var serializedMessage = JsonConvert.SerializeObject(message);
                var deliveryReport = await _producer.ProduceAsync(
                    topic,
                    new Message<Null, string> { Value = serializedMessage }
                );

                if (deliveryReport.Status == PersistenceStatus.NotPersisted)
                {
                    throw new ProduceException<Null, string>(
                        new Error(ErrorCode.Local_TimedOut, "Message timeout"),
                        deliveryReport
                    );
                }
            }
            catch (ProduceException<Null, string> e)
            {
                _logger.LogError(e, "Kafka delivery failed: {Reason}", e.Error.Reason);
                throw;
            }
            catch (KafkaException ex)
            {
                _logger.LogError(ex, "Kafka connection error: {Message}", ex.Message);
                throw new Exception("Kafka connection failed", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Kafka producer");
                throw;
            }
        }
    }
}