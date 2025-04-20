using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaPay.Shared.Domain.Common
{
    public interface IAuditableEntity
    {
       public DateTimeOffset Created { get; set; }
       public DateTimeOffset LastModified { get; set; }

    }
}
