using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaPay.Shared.Application.Common.Interfaces
{
    public interface IKafkaProducer
    {
        Task ProduceAsync(string topic, string message);

    }
}
