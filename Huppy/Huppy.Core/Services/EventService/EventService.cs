using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.EventService
{
    public record Event(object? Data, string Name, Func<object?, Task> Task);
    public class EventService : IEventService
    {
        public event Func<string[], Task> OnEventsRemoved;
        private readonly ILogger _logger;
        public readonly ConcurrentDictionary<ulong, List<Event>> jobs = new();
        private readonly SemaphoreSlim _semiphore = new(4);
        private Timer? _timer;
        private DateTime _startDate;
        private readonly TimeSpan _ticker = TimeSpan.FromSeconds(1); // ticks
        private const int TICKS_PER_SECOND = 10000000;

        public EventService(ILogger<EventService> logger)
        {
            _logger = logger;
        }

        public void Initialize()
        {
            _logger.LogInformation("Starting Event Loop Service");

            _startDate = DateTime.Now;
            _timer = InitTimer();
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

        public async Task AddRange(DateTime time, ICollection<Event> eventJobs)
        {
            var targetTime = GetTargetTime(time);
            await _semiphore.WaitAsync();

            if (jobs.ContainsKey(targetTime))
            {
                jobs.TryGetValue(targetTime, out var value);
                value ??= new();
                value.AddRange(eventJobs);
            }
            else
            {
                jobs.TryAdd(targetTime, eventJobs.ToList());
            }

            _semiphore.Release();
        }

        // To remake
        // public Task RemovePrecise(DateTime? time, string name)
        // {
        //     if (string.IsNullOrEmpty(name) || time is null) return Task.CompletedTask;

        //     var targetTime = GetTargetTime((DateTime)time);
        //     if (events.ContainsKey(targetTime))
        //     {
        //         events.TryGetValue(targetTime, out var eventList);
        //         if (eventList is null)
        //         {
        //             _logger.LogError("Precise remove error {time} <{name}>: event list was null", time, name);
        //             return Task.CompletedTask;
        //         }

        //         var eventToRemove = eventList.FirstOrDefault(e => e.Name == name);
        //         if (eventToRemove is null)
        //         {
        //             _logger.LogError("Precise remove error {time} <{name}>: didn't find event with provided name", time, name);
        //             return Task.CompletedTask;
        //         }

        //         eventList.Remove(eventToRemove);

        //         lock (_lockObj)
        //         {
        //             events[targetTime] = eventList;
        //         }
        //     }

        //     return Task.CompletedTask;
        // }

        // public Task RemoveEventsByName(string name)
        // {
        //     if (string.IsNullOrEmpty(name)) return Task.CompletedTask;

        //     var eventsToRemove = events.Where(e => e.Value.Any(e => e.Name == name));

        //     foreach (var time in eventsToRemove)
        //     {
        //         events.Remove(time.Key);
        //     }

        //     return Task.CompletedTask;
        // }

        // public Task RemoveEventsByTime(DateTime time)
        // {
        //     var targetTime = GetTargetTime(time);
        //     if (events.ContainsKey(targetTime))
        //     {
        //         lock (_lockObj)
        //         {
        //             if (!events.Remove(targetTime))
        //                 _logger.LogError("Failed to removed events at {time}", targetTime);
        //         }
        //     }

        //     return Task.CompletedTask;
        // }

        public DateTime GetStartTime() => _startDate;

        private Timer InitTimer()
        {
            var timer = new Timer(async (e) =>
            {
                await Execute();
            }, null, new TimeSpan(0), _ticker);

            return timer;
        }

        private async Task Execute()
        {
            try
            {
                ConcurrentBag<string> removedNames = new();

                var targetTime = GetTargetTime(DateTime.UtcNow);
                var executionJobs = jobs.Where(job => job.Key < targetTime);
                if (!executionJobs.Any()) return;

                ParallelOptions parallelOptions = new()
                {
                    MaxDegreeOfParallelism = 4
                };

                await Parallel.ForEachAsync(executionJobs, parallelOptions, async (exeJobs, token) =>
                {
                    var tasks = exeJobs.Value
                        .Select(task => Task.Run(async () => await task.Task(task.Data)))
                        .ToList();

                    await Task.WhenAll(tasks);

                    jobs.TryRemove(exeJobs.Key, out var removed);

                    if (removed is null) return;
                    foreach (var name in removed.Select(e => e.Name)) removedNames.Add(name);
                });

                OnEventsRemoved?.Invoke(removedNames.ToArray());
            }
            catch (Exception) { }
        }

        private static ulong GetTargetTime(DateTime time)
        {
            return (ulong)(time.Ticks / TICKS_PER_SECOND);
        }
    }
}