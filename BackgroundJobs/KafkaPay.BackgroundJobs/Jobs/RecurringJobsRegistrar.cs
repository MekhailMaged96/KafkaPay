using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using KafkaPay.BackgroundJobs.Services;

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
            _jobManager.AddOrUpdate<ITasksService>(
                "daily-report-job",
                service => service.ProcessDailyReportAsync(),
                Cron.Minutely);
        
        }
   
    }
}
