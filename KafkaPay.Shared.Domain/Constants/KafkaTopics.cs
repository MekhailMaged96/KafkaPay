using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaPay.Shared.Domain.Constants
{
    public abstract  class KafkaTopics
    {
        public const string TransactionTopic = nameof(TransactionTopic);
    }
}
