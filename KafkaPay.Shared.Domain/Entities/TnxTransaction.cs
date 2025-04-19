using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using KafkaPay.Shared.Domain.Common;

namespace KafkaPay.Shared.Domain.Entities
{
    public class TnxTransaction : BaseAuditableEntity<Guid>
    {
        public Guid FromAccountId { get; set; }
        public Guid ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public int StatusId { get; set; }
        [ForeignKey(nameof(StatusId))]
        public TransactionStatus Status { get; set; }

        [ForeignKey(nameof(FromAccountId))]
        public Account FromAccount { get; set; }
        [ForeignKey(nameof(ToAccountId))]
        public Account ToAccount { get; set; }
    }
}
