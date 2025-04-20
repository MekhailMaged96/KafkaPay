using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using KafkaPay.Shared.Application.Common.Interfaces;

namespace KafkaPay.Shared.Infrastructure.MessageBrokers
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<string, string> _producer;

        public KafkaProducer(string brokerList)
        {
            var config = new ProducerConfig { BootstrapServers = brokerList };
            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task ProduceAsync(string topic, string message)
        {
            try
            {
                await _producer.ProduceAsync(topic, new Message<string, string> { Key = null, Value = message });
            }
            catch (ProduceException<string, string> e)
            {
                // Handle error
                Console.WriteLine($"Error producing message: {e.Message}");
            }
        }
    }
}
