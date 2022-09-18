using System.Runtime.CompilerServices;
using Huppy.Core.Entities;
using Huppy.Core.Interfaces.IServices;
using Huppy.Kernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.TimedEvents;

public class TimedEventsService : ITimedEventsService
{
    // public event Func<Task?> OnJobAdded;
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    // private readonly Dictionary<Guid, Timer> _timers;
    private readonly Dictionary<Guid, TimedJobTimer> _timers = new();
    // private readonly AppSettings _settings;
    // private readonly List<Job> _jobs;
    // private readonly List<TimedJob> _jobs1;
    private record TimedJobTimer(TimedJob Job, Timer Timer);
    // private record Job(Guid Id, Func<AsyncServiceScope, object?, Task?> Function, object? Data, TimeSpan DueTime, TimeSpan Period) { };
    public TimedEventsService(ILogger<TimedEventsService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        // _settings = settings;
        // _timers = new();
        // _jobs = new();
        // OnJobAdded += OnJobAddedInternal;
    }

    // public Task StartTimers()
    // {
    //     _logger.LogInformation("Starting Timed Events Service");

    //     // foreach (var job in _jobs) AddTimer(job);

    //     return Task.CompletedTask;
    // }

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

    // public void AddJob(Guid jobGuid, object? data, TimeSpan dueTime, TimeSpan period, Func<AsyncServiceScope, object?, Task?> task)
    // {
    //     Job job = new(jobGuid, task, data, dueTime, period);
    //     _jobs.Add(job);
    //     OnJobAdded?.Invoke();
    // }

    // private Task OnJobAddedInternal()
    // {
    //     foreach (var job in _jobs) AddTimer(job);

    //     return Task.CompletedTask;
    // }

    // private void AddTimer(Job job)
    // {
    //     if (_timers.TryGetValue(job.Id, out _)) return;

    //     _timers.Add(job.Id, new Timer(async (e) =>
    //     {
    //         using var scope = _scopeFactory.CreateAsyncScope();
    //         await job.Function.Invoke(scope, job.Data)!;
    //     }, null, job.DueTime, job.Period));

    //     _logger.LogInformation("Started job: [ID: {id}] [DueTime: {dueTime}] [Period: {period}]", job.Id, job.DueTime, job.Period);
    // }
}
