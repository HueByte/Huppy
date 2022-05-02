using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.EventService
{
    public class EventService : IEventService
    {
        private readonly ILogger _logger;
        public readonly Dictionary<ulong, List<Func<Task>>> events = new();
        private readonly object _lockObj = new();
        private Timer _timer;
        private DateTime _startDate;
        private readonly TimeSpan _ticker = TimeSpan.FromSeconds(1); // ticks
        private const int TICKS_PER_SECOND = 10000000;

        public EventService(ILogger<EventService> logger)
        {
            _logger = logger;
        }

        public void Initialize()
        {
            _startDate = DateTime.Now;
            _timer = InitTimer();

            _logger.LogInformation("Started event loop");
        }

        public void AddEvent(DateTime time, Func<Task> action)
        {
            AddRange(time, new List<Func<Task>> { action });
        }

        public void AddEvent(DateTime time, Task action)
        {
            AddRange(time, new List<Func<Task>> { async () => await action });
        }

        public DateTime GetStartTime() => _startDate;

        public void AddRange(DateTime time, List<Func<Task>> actions)
        {
            lock (_lockObj)
            {
                var targetTime = (ulong)(time.Ticks / TICKS_PER_SECOND);
                if (events.ContainsKey(targetTime))
                {
                    events.TryGetValue(targetTime, out var value);
                    value ??= new();
                    value.AddRange(actions);
                }

                events.TryAdd(targetTime, actions);
            }
        }

        private Timer InitTimer()
        {
            var timer = new Timer(async (e) =>
            {
                await ExecuteEvents();
            }, null, new TimeSpan(0), _ticker);

            return timer;
        }

        private async Task ExecuteEvents()
        {
            try
            {
                var targetTime = (ulong)(DateTime.Now.Ticks / TICKS_PER_SECOND);
                var executionEvents = events.Where(e => e.Key < targetTime);

                if (executionEvents.Any())
                {
                    foreach (var tasks in executionEvents)
                    {
                        foreach (var task in tasks.Value)
                        {
                            _ = Task.Run(task);
                        }
                    }
                }
                foreach (var key in executionEvents)
                {
                    lock (_lockObj)
                    {
                        events.Remove(key.Key);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("{message} - {stackTrace}", e.Message, e.StackTrace);
            }
        }
    }
}