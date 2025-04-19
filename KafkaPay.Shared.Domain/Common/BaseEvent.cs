using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace KafkaPay.Shared.Domain.Common
{
    public abstract class BaseEvent : INotification
    {
    }
}
