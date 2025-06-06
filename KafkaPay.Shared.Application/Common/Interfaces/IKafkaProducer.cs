﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace KafkaPay.Shared.Application.Common.Interfaces
{
    public interface IKafkaProducer<T>
    {
        Task ProduceAsync(string topic, T message);
    }
}
