using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using KafkaPay.Shared.Application.Common.Interfaces;

namespace KafkaPay.Shared.Infrastructure.MessageBrokers
{
    public class KafkaConsumer : IKafkaConsumer
    {
        private readonly IConsumer<string, string> _consumer;

        public KafkaConsumer(string brokerList)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = brokerList,
                GroupId = "your-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<string, string>(config).Build();
        }

        public async Task ConsumeAsync(string topic, CancellationToken cancellationToken)
        {
            _consumer.Subscribe(topic);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken);
                    Console.WriteLine($"Consumed message: {consumeResult.Message.Value}");
                    // Process the consumed message
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Error consuming message: {e.Error.Reason}");
                }
            }
        }
    }
}
