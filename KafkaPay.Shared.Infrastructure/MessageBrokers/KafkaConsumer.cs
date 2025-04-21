using System.Text;
using Confluent.Kafka;
using KafkaPay.Shared.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

            _consumer = new ConsumerBuilder<TKey, TValue>(config)
                .SetKeyDeserializer(GetKeyDeserializer())
                .SetValueDeserializer(new JsonDeserializer<TValue>())
                .Build();

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
                _logger.LogError(ex, $"Error consuming message from topic {ex.ConsumerRecord?.Topic}");
                throw;
            }
        }

        public void Close()
        {
            _consumer.Close();
        }

        private IDeserializer<TKey> GetKeyDeserializer()
        {
            if (typeof(TKey) == typeof(string)) return (IDeserializer<TKey>)Deserializers.Utf8;
            if (typeof(TKey) == typeof(Ignore)) return (IDeserializer<TKey>)Deserializers.Ignore;

            throw new NotSupportedException($"Deserializer for type {typeof(TKey).Name} is not supported.");
        }
    }

    public class JsonDeserializer<T> : IDeserializer<T>
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            if (isNull) return default;
            var json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
