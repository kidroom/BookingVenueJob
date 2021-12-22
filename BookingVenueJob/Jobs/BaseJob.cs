using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingVenueJob.Jobs
{
    public abstract class BaseJob : IJob
    {
        private readonly ILogger<BaseJob> _logger;

        public BaseJob(ILogger<BaseJob> logger)
        {
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            // Init ActivityId for Nlog
            Trace.CorrelationManager.ActivityId = Guid.NewGuid();

            _logger.LogInformation($"Job Start - {GetType().Name}");

            await DoJob(context);

            _logger.LogInformation($"Job End - {GetType().Name}");
        }

        public abstract Task DoJob(IJobExecutionContext context);
    }
}
