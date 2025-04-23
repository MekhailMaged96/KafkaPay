using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KafkaPay.Shared.Domain.Common;

namespace KafkaPay.Shared.Domain.Events
{
    public class TransactionInitiEvent : BaseEvent
    {
        public Guid TransactionId { get; set; }
        public Guid? FromAccountId { get; set; }
        public Guid? ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }
    public record TransactionInitiatedEvent(
     Guid TransactionId,
     Guid? FromAccountId,
     Guid? ToAccountId,
     decimal Amount,
     DateTime Timestamp);

    public record TransactionCompletedEvent(
        Guid TransactionId,
        DateTime CompletedAt);

    public record TransactionFailedEvent(
        Guid TransactionId,
        string Reason,
        DateTime FailedAt);
}
