using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KafkaPay.TransferService.Infrastructure.Backgrounds
{
    public sealed class TransactionKafkaHostedService : BackgroundService
    {

        private readonly ILogger<TransactionKafkaHostedService> _logger;
        private readonly TransactionEventConsumer _transactionEventConsumer;

        public TransactionKafkaHostedService(
            ILogger<TransactionKafkaHostedService> logger,
            TransactionEventConsumer transactionEventConsumer)
        {
            _logger = logger;
            _transactionEventConsumer = transactionEventConsumer;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Kafka Background Service is starting.");
             return Task.Run(async () => await _transactionEventConsumer.ConsumeMessagesAsync(stoppingToken));
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Kafka Background Service is stopping.");
            return base.StopAsync(cancellationToken);
        }
      
    }
}
