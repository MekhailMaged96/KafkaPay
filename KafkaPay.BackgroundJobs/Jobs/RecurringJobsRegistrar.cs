using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using KafkaPay.Shared.Infrastructure.Backgrounds.Jobs;

namespace KafkaPay.BackgroundJobs.Jobs
{
    public class RecurringJobsRegistrar : IRecurringJobRegistrar
    {
        private readonly IRecurringJobManager _jobManager;

        public RecurringJobsRegistrar(IRecurringJobManager jobManager)
        {
            _jobManager = jobManager;
        }

        public void RegisterJobs()
        {
          
            _jobManager.AddOrUpdate<ProcessOutboxMessageJob>(
               "Process-OutboxMessage-Job",
               service => service.Execute(),
               "*/5 * * * * *");
        }
   
    }
}
