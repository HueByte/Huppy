using Huppy.Core.Entities;
using Huppy.Core.Interfaces.IServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.TimedEvents;

public class TimedEventsService : ITimedEventsService
{
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Dictionary<Guid, TimedJobTimer> _timers = new();
    private record TimedJobTimer(TimedJob Job, Timer Timer);
    public TimedEventsService(ILogger<TimedEventsService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public void AddJob(TimedJob job) => AddTimer(job);

    public Task RemoveJob(Guid jobId)
    {
        _timers.TryGetValue(jobId, out var timedJob);

        timedJob?.Timer.Dispose();
        _timers.Remove(jobId);
        _logger.LogInformation("Job removed [ID: {jobId}] [Name: {jobName}]", timedJob?.Job.JobId, timedJob?.Job.Name);

        return Task.CompletedTask;
    }

    private void AddTimer(TimedJob job)
    {
        if (_timers.TryGetValue(job.JobId, out _)) return;
        if (job.Function is null)
        {
            _logger.LogError("Timed job function cannot be null");
            return;
        }

        _timers.Add(job.JobId, new TimedJobTimer(job, new Timer(async (args) =>
        {
            using var scope = _scopeFactory.CreateAsyncScope();
            await job.Function?.Invoke(scope, job.Data!)!;
        }, null, job.DueTime, job.Period)));

        _logger.LogInformation("Started job: [ID: {jobId}] [Name: {jobName}] [DueTime: {dueTime} | Period: {period}]", job.JobId, job.Name, job.DueTime, job.Period);
    }
}
