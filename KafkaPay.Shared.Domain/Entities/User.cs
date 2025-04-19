using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using KafkaPay.Shared.Domain.Common;

namespace KafkaPay.Shared.Domain.Entities
{
    public class User : BaseAuditableEntity<Guid>
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public ICollection<Account> Accounts { get; set; } = new List<Account>();

    }
}
