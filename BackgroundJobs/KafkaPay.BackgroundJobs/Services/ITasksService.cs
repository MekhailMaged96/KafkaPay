using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaPay.BackgroundJobs.Services
{
    public interface ITasksService
    {
        Task ProcessDailyReportAsync();
    }
}
