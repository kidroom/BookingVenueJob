using BookingVenueJob.Model;
using BookingVenueJob.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using Quartz;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace BookingVenueJob
{
    class Program
    {
        private static IConfiguration _config;

        static async Task Main(string[] args)
        {
            _config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false)
                    .AddEnvironmentVariables()
                    .Build();

            LogManager.Configuration = new NLogLoggingConfiguration(_config.GetSection("NLog"));

            var logger = LogManager.GetCurrentClassLogger();

            try
            {
                logger.Info("Scheduler Start");

                CreateHostBuilder(args).Build().Run();

                logger.Info("Scheduler End");
            }
            catch (Exception ex)
            {
                // NLog: catch any exception and log it.
                logger.Error(ex, "Scheduler Stopped with exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Add the required Quartz.NET services
                    services.AddQuartz(q =>
                    {
                        // Use a Scoped container to create jobs. I'll touch on this later
                        q.UseMicrosoftDependencyInjectionScopedJobFactory();
                    });

                    // Add the Quartz.NET hosted service
                    services.AddQuartzHostedService(
                            q => q.WaitForJobsToComplete = true);

                    // 2. 註冊服務
                    services.AddSingleton(_config);
                    services.AddLogging(loggingBuilder =>
                    {
                        // configure Logging with NLog
                        loggingBuilder.ClearProviders();
                        loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                        loggingBuilder.AddNLog(_config.GetSection("NLog"));
                    });

                    ConfigureJobs(services);

                    // Add the Quartz.NET hosted service
                    services.AddQuartzHostedService(
                        q => q.WaitForJobsToComplete = true);
                });
        }
        private static void ConfigureJobs(IServiceCollection services)
        {
            // Add the required Quartz.NET services
            services.AddQuartz(q =>
            {
                // Use a Scoped container to create jobs. I'll touch on this later
                q.UseMicrosoftDependencyInjectionScopedJobFactory();
                q.UseDefaultThreadPool(tp =>
                {
                    tp.MaxConcurrency = 50;
                });

                var ns = "BookingVenueJob.Jobs";

                var types = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.Namespace == ns && t.IsPublic)
                    .ToList();


                var scheduleJobMethodInstance = typeof(Quartz.ServiceCollectionExtensions).GetMethod("ScheduleJob");

                foreach (var type in types)
                {
                    var scheduleJobGenericMethodInstance = scheduleJobMethodInstance.MakeGenericMethod(type);

                    var setTriggerFunc = MakeSetTriggerFunc(type.Name);
                    var setJobFunc = MakeSetJobFunc(type.Name);

                    scheduleJobGenericMethodInstance.Invoke(null, new object[] { q, setTriggerFunc, setJobFunc });
                }
            });

            services.Configure<ProjectSetting>(_config.GetSection("ProjectSetting"));

            services.AddHttpClient();

            services.AddScoped<IBadmintonService, BadmintonService>();
        }
        private static Action<ITriggerConfigurator> MakeSetTriggerFunc(string jobName)
        {
            var groupName = jobName;
            var cronSchedule = _config.GetValue<string>($"Jobs:{jobName}");

            return (trigger) =>
            {
                trigger.WithIdentity("Trigger", groupName).WithCronSchedule(cronSchedule);
            };
        }

        private static Action<IJobConfigurator> MakeSetJobFunc(string jobName)
        {
            var groupName = jobName;

            return (job) =>
            {
                job.WithIdentity("Job", groupName);
            };
        }
    }
}
