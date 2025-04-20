using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaPay.Shared.Domain.Enums
{
    public enum TnxTransactionStatusEnum
    {
        Pending=1,
        Completed,
        Failed,
        Cancelled,
    }
}
