using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KafkaPay.Shared.Domain.Common;

namespace KafkaPay.Shared.Domain.Entities
{
    public class Account : BaseAuditableEntity<Guid>
    {
        public decimal Balance { get; set; }
        public string Currency { get; set; } = "USD";

        public Guid UserId { get; set; }

        public User User { get; set; }

        public ICollection<TnxTransaction> TnxTransactions { get; set; } = new List<TnxTransaction>();
    }
}
