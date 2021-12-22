using BookingVenueJob.Services;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingVenueJob.Jobs
{
    [DisallowConcurrentExecution]
    public class AutoBookingJob : BaseJob
    {
        private readonly ILogger<AutoBookingJob> _logger;
        private readonly BadmintonService _badmintonService;
        public AutoBookingJob(ILogger<AutoBookingJob> logger,
                              BadmintonService badmintonService) : base(logger)
        {
            _logger = logger;
            _badmintonService = badmintonService;
        }
        public override Task DoJob(IJobExecutionContext context)
        {
            _badmintonService.BookByUrl();
            return Task.CompletedTask;
        }
    }
}
