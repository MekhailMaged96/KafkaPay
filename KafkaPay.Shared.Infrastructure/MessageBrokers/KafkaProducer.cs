using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly Action<IProducer<Null, string>, Error> _errorHandler;
        public KafkaProducer(IConfiguration configuration,ILogger<KafkaProducer<T>> logger)
        {
            var config = new ProducerConfig { BootstrapServers = configuration["KafkaSettings:BootstrapServers"],EnableIdempotence = true };
       
            _logger = logger;

            _producer = new ProducerBuilder<Null, string>(config)
                 .SetErrorHandler((producer, error) =>
                 {
                     _logger.LogError("Kafka producer error: {ErrorCode} - {ErrorMessage}",
                                    error.Code, error.Reason);
                 })
                 .Build();

        }

        public async Task ProduceAsync(string topic, T message)
        {
            try
            {
                var serializedMessage = JsonConvert.SerializeObject(message);

                var deliveryResult = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = serializedMessage });

            }
            catch (ProduceException<Null, string> e)
            {
                _logger.LogError($"Delivery failed: {e.Error.Reason}");
            }
        }

    

    }
}
