using System.Runtime.CompilerServices;
using Huppy.Core.Entities;
using Huppy.Core.Services.NewsService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.TimedEventsService
{
    public class TimedEventsService : ITimedEventsService
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Dictionary<Guid, Timer> _timers;
        private readonly AppSettings _settings;
        private record TimerJob(Func<AsyncServiceScope, Task?> Job, TimeSpan DueTime, TimeSpan Period) { };
        private readonly List<TimerJob> _workers;
        public TimedEventsService(ILogger<TimedEventsService> logger, IServiceScopeFactory scopeFactory, AppSettings settings)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _settings = settings;
            _timers = new();
            _workers = new();
        }

        public Task StartTimers()
        {
            _logger.LogInformation("Starting Timed Events");
            foreach (var worker in _workers)
            {
                var guid = Guid.NewGuid();
                _timers.Add(guid, new Timer(async (e) =>
                {
                    using var scope = _scopeFactory.CreateAsyncScope();
                    await worker.Job.Invoke(scope)!;
                }, null, worker.DueTime, worker.Period));

                _logger.LogInformation("Started job: [{id}] [{dueTime}] [{period}]", guid, worker.DueTime, worker.Period);
            }

            return Task.CompletedTask;
        }

        public void AddJob(Func<AsyncServiceScope, Task?> task, TimeSpan dueTime, TimeSpan period)
        {
            TimerJob job = new(task, dueTime, period);
            _workers.Add(job);
        }
    }
}