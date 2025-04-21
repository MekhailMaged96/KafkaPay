using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaPay.Shared.Domain.Events
{
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
