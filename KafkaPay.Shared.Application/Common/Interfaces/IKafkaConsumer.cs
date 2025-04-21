using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace KafkaPay.Shared.Application.Common.Interfaces
{
    public interface IKafkaConsumer<TKey, TValue>
    {
        void Subscribe(string topic);
        ConsumeResult<TKey, TValue> Consume(CancellationToken cancellationToken);
        void Close();

    }
}
