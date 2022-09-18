using Huppy.Core.Entities;
using Huppy.Core.Interfaces.IServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.JobManager
{
    public class JobManagerService : IJobManagerService
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITimedEventsService _timedEventsService;
        public Dictionary<Guid, string> RegisteredJobs = new();
        private bool _isInitialized;
        public JobManagerService(ILogger<JobManagerService> logger, IServiceScopeFactory scopeFactory, ITimedEventsService timedEventsService)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _timedEventsService = timedEventsService;
        }

        public Task StartEventLoop()
        {
            _logger.LogInformation("JobManager :: Starting event loop service job");

            using var scope = _scopeFactory.CreateAsyncScope();
            var eventLoopServiceTemp = scope.ServiceProvider.GetRequiredService<IEventLoopService>();

            TimedJob job = new()
            {
                JobId = Guid.NewGuid(),
                Name = "Event Loop",
                Data = null,
                DueTime = new TimeSpan(0),
                Period = eventLoopServiceTemp.EventLoopExecutionFrequency,
                Function = async (scope, data) =>
                {
                    var eventLoopService = scope.ServiceProvider.GetRequiredService<IEventLoopService>();
                    await eventLoopService.Execute();
                }
            };

            _timedEventsService.AddJob(job);
            RegisteredJobs.Add(job.JobId, job.Name);
            return Task.CompletedTask;
        }

        public Task StartReminderJobs()
        {
            _logger.LogInformation("JobManager :: Starting reminder service job");

            using var scope = _scopeFactory.CreateAsyncScope();
            var reminderServiceTemp = scope.ServiceProvider.GetRequiredService<IReminderService>();

            TimedJob job = new()
            {
                JobId = Guid.NewGuid(),
                Name = "Reminder Fetcher",
                Data = null,
                DueTime = new TimeSpan(0),
                Period = reminderServiceTemp.FetchReminderFrequency,
                Function = async (scope, data) =>
                {
                    var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();
                    await reminderService.RegisterFreshReminders();
                }
            };

            _timedEventsService.AddJob(job);
            RegisteredJobs.Add(job.JobId, job.Name);
            return Task.CompletedTask;
        }

        public Task StartActivityControlJobs()
        {
            _logger.LogInformation("JobManager :: Starting activity control service job");

            using var scope = _scopeFactory.CreateAsyncScope();
            var activityControlServiceTemp = scope.ServiceProvider.GetRequiredService<IActivityControlService>();

            TimedJob job = new()
            {
                JobId = Guid.NewGuid(),
                Name = "Activity Control",
                Data = null,
                DueTime = new TimeSpan(0),
                Period = activityControlServiceTemp.UpdateStatusFrequency,
                Function = async (scope, data) =>
                {
                    var activityControlService = scope.ServiceProvider.GetRequiredService<IActivityControlService>();
                    await activityControlService.ChangeActivity(scope);
                }
            };

            _timedEventsService.AddJob(job);
            RegisteredJobs.Add(job.JobId, job.Name);
            return Task.CompletedTask;
        }

        public void MarkInitialized() => _isInitialized = true;
    }
}