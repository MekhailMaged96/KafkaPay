using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaPay.BackgroundJobs.Jobs
{
    public interface IRecurringJobRegistrar
    {
        void RegisterJobs();
    }
}
