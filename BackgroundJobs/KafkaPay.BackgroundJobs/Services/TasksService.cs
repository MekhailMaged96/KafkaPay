using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaPay.BackgroundJobs.Services
{
    public class TasksService : ITasksService
    {
        private readonly ILogger<TasksService> _logger;

        public TasksService(ILogger<TasksService> logger)
        {
            _logger = logger;
        }

        public Task ProcessDailyReportAsync()
        {
            _logger.LogInformation("Processing daily report at {time}", DateTime.Now);
            return Task.Delay(1000); // Simulate work
        }

    }
}
