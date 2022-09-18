using System.Collections.Concurrent;
using System.Diagnostics;
using Huppy.Core.Interfaces.IServices;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.EventLoop;

public record Event(object? Data, string Name, Func<object?, Task> Task);
public class EventLoopService : IEventLoopService
{
    public event Func<string[], Task>? OnEventsRemoved;
    public TimeSpan EventLoopExecutionFrequency { get; } = TimeSpan.FromSeconds(10); // ticks
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<ulong, List<Event>> jobsQueue = new();
    private readonly SemaphoreSlim _semiphore = new(1, 1);
    private readonly int _maxDegreeOfParallelism = Environment.ProcessorCount;
    private readonly ConcurrentBag<string> _removedNames = new();
    private const int TICKS_PER_SECOND = 10_000_000;

    public EventLoopService(ILogger<EventLoopService> logger)
    {
        _logger = logger;
    }

    public async Task AddEvent(DateTime time, string Name, object? data, Func<object?, Task> job)
    {
        Event evn = new(data, Name, job);
        await AddEvent(time, evn);
    }

    public async Task AddEvent(DateTime time, Event job)
    {
        await AddRange(time, new List<Event>() { job });
    }

    public Task AddRange(DateTime time, ICollection<Event> eventJobs)
    {
        var targetTime = GetTargetTime(time);
        try
        {
            if (jobsQueue.TryGetValue(targetTime, out var value))
            {
                value ??= new();
                value.AddRange(eventJobs);
                _logger.LogInformation("Appending {count} jobs to event loop", eventJobs.Count);
            }
            else
            {
                jobsQueue.TryAdd(targetTime, eventJobs.ToList());
                _logger.LogInformation("Adding {count} jobs to event loop", eventJobs.Count);
            }
        }
        catch (Exception ex) { _logger.LogError("Event loop adding event error", ex); }

        return Task.CompletedTask;
    }

    public async Task Remove(DateTime time, string eventName)
    {
        var targetTime = GetTargetTime(time);
        await Remove(targetTime, eventName);
    }

    public Task Remove(ulong time, string eventName)
    {
        jobsQueue.TryGetValue(time, out var value);

        if (value is null) return Task.CompletedTask;

        var entity = value.FirstOrDefault(e => e.Name == eventName);

        if (entity is not null) value.Remove(entity);

        return Task.CompletedTask;
    }

    public async Task Execute()
    {
        _logger.LogDebug("Starting event loop execution");

        await _semiphore.WaitAsync();
        bool isSemiphoreReleased = false;
        Stopwatch watch = new();
        watch.Start();

        try
        {
            var jobs = jobsQueue.Where(job => job.Key < GetTargetTime(DateTime.UtcNow));
            // ConcurrentBag<string> _removedNames = new();

            if (!jobs.Any())
            {
                _semiphore.Release();
                isSemiphoreReleased = true;
                return;
            }

            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = _maxDegreeOfParallelism
            };

            // iterate in async parallel each queued jobs that met the condition 
            await Parallel.ForEachAsync(jobs, parallelOptions, async (iteratedQueue, token) =>
            {
                // start all events that job contains and await it
                var tasks = iteratedQueue.Value
                    .Select(job => Task.Run(async () => await job.Task(job.Data)))
                    .ToList();

                await Task.WhenAll(tasks);

                jobsQueue.TryRemove(iteratedQueue.Key, out var removedJobs);

                if (removedJobs is null) return;
                foreach (var name in removedJobs.Select(e => e.Name)) _removedNames.Add(name);
            });

            OnEventsRemoved?.Invoke(_removedNames.ToArray());
            _logger.LogInformation("Executed {noEvents} event loop jobs in {time}", _removedNames.Count, watch.Elapsed);
        }
        catch (Exception ex) { _logger.LogError("Event loop error", ex); }
        finally
        {
            watch.Stop();
            _removedNames.Clear();
            if (!isSemiphoreReleased) _semiphore.Release(1);
            _logger.LogDebug("Finished event loop execution");
        }
    }

    private static ulong GetTargetTime(DateTime time)
    {
        return (ulong)(time.Ticks / TICKS_PER_SECOND);
    }
}