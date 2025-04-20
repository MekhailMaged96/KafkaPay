using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaPay.Shared.Application.Common.Interfaces
{
    public interface IKafkaConsumer
    {
        Task ConsumeAsync(string topic, CancellationToken cancellationToken);
    }
}
