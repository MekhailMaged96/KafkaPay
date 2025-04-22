using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KafkaPay.Shared.Infrastructure.Consumers
{
    public abstract class KafkaBaseConsumer<TKey, TValue>
    {
        private readonly IConsumer<TKey, TValue> _consumer;
        protected readonly ILogger<KafkaBaseConsumer<TKey, TValue>> _logger;

        protected KafkaBaseConsumer(
            ConsumerConfig config,
            ILogger<KafkaBaseConsumer<TKey, TValue>> logger,
            IDeserializer<TKey> keyDeserializer = null,
            IDeserializer<TValue> valueDeserializer = null)
        {
            _logger = logger;

            // Use default bootstrap server if none is provided
            if (string.IsNullOrEmpty(config.BootstrapServers))
            {
                config.BootstrapServers = "localhost:9092";
            }

            keyDeserializer ??= GetKeyDeserializer();
            valueDeserializer ??= new JsonDeserializer<TValue>();

            _consumer = new ConsumerBuilder<TKey, TValue>(config)
                .SetKeyDeserializer(keyDeserializer)
                .SetValueDeserializer(valueDeserializer)
                .Build();
        }

        public void Subscribe(string topic) => _consumer.Subscribe(topic);

        public ConsumeResult<TKey, TValue> Consume(TimeSpan timeout)
        {
            try
            {
                return _consumer.Consume(timeout);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, $"Error consuming message from topic {ex.ConsumerRecord?.Topic}");
                throw;
            }
        }

        public ConsumeResult<TKey, TValue> Consume(CancellationToken cancellationToken)
        {
            try
            {
                return _consumer.Consume(cancellationToken);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, $"Error consuming message from topic {ex.ConsumerRecord?.Topic}");
                throw;
            }
        }

        public void Close() => _consumer.Close();

        public void Commit(ConsumeResult<TKey, TValue> result)
        {
            try
            {
                _consumer.Commit(result);
            }
            catch (KafkaException ex)
            {
                _logger.LogError(ex, $"Error committing offset for topic {result.Topic}");
                throw;
            }
        }

        private IDeserializer<TKey> GetKeyDeserializer()
        {
            if (typeof(TKey) == typeof(string))
                return (IDeserializer<TKey>)Deserializers.Utf8;
            if (typeof(TKey) == typeof(Ignore))
                return (IDeserializer<TKey>)Deserializers.Ignore;

            throw new NotSupportedException($"Deserializer for type {typeof(TKey).Name} is not supported.");
        }

        protected class JsonDeserializer<T> : IDeserializer<T>
        {
            public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
            {
                if (isNull) return default;
                var json = Encoding.UTF8.GetString(data);
                return JsonConvert.DeserializeObject<T>(json);
            }
        }
    }
}
