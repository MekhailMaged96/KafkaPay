using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using KafkaPay.Shared.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace KafkaPay.Shared.Infrastructure.MessageBrokers
{
    public class KafkaProducer<T> : IKafkaProducer<T>
    {
        private readonly IProducer<Null, string> _producer;

        public KafkaProducer(IConfiguration configuration)
        {
            var config = new ProducerConfig { BootstrapServers = configuration["KafkaSettings:BootstrapServers"] };
            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public async Task ProduceAsync(string topic, T message)
        {
            try
            {
                var serializedMessage = JsonConvert.SerializeObject(message);
                var deliveryResult = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = serializedMessage });
                Console.WriteLine($"Delivered '{deliveryResult.Value}' to '{deliveryResult.TopicPartitionOffset}'");
            }
            catch (ProduceException<Null, string> e)
            {
                Console.WriteLine($"Error producing message: {e.Error.Reason}");
            }
        }
    }
}
