using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using KafkaPay.Shared.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KafkaPay.Shared.Infrastructure.MessageBrokers
{
    public class KafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue>
        {
        private readonly IConsumer<TKey, TValue> _consumer;
        private readonly ILogger<KafkaConsumer<TKey, TValue>> _logger;

        public KafkaConsumer(ILogger<KafkaConsumer<TKey, TValue>> logger)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "background-jobs-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<TKey, TValue>(config).Build();
            _logger = logger;
        }
        public void Subscribe(string topic)
        {
            _consumer.Subscribe(topic);
        }

        public ConsumeResult<TKey, TValue> Consume(CancellationToken cancellationToken)
        {
            try
            {
                return _consumer.Consume(cancellationToken);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, $"Error consuming message from topic {ex.ConsumerRecord.Topic}");
                throw;
            }
        }
        public void Close()
        {
            _consumer.Close();
        }
    }
}
