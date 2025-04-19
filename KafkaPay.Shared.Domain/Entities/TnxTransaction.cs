using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using KafkaPay.Shared.Domain.Common;
using KafkaPay.Shared.Domain.Enums;

namespace KafkaPay.Shared.Domain.Entities
{
    public class TnxTransaction : BaseAuditableEntity<Guid>
    {
        public Guid FromAccountId { get; set; }
        public Guid ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public TnxTransactionStatus Status { get; set; }

        public Account FromAccount { get; set; }
        public Account ToAccount { get; set; }
    }
}
