using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.EventService
{
    public class EventService : IEventService
    {
        private readonly ILogger _logger;
        public readonly Dictionary<ulong, List<TimedEvent>> events = new();
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

        public void AddEvent(DateTime time, TimedEvent action)
        {
            AddRange(time, new List<TimedEvent> { action });
        }

        public void AddEvent(DateTime time, string eventName, Func<Task> action)
        {
            List<TimedEvent> newEvents = new()
            {
                new TimedEvent {
                    Name = eventName,
                    Event = action
                }
            };

            AddRange(time, newEvents);
        }

        public void AddEvent(DateTime time, string eventName, Task action)
        {
            List<TimedEvent> newEvents = new()
            {
                new TimedEvent {
                    Name = eventName,
                    Event = async () => await action
                }
            };

            AddRange(time, newEvents);
        }

        public void AddRange(DateTime time, List<TimedEvent> actions)
        {
            var targetTime = GetTargetTime(DateTime.Now);
            lock (_lockObj)
            {
                if (events.ContainsKey(targetTime))
                {
                    events.TryGetValue(targetTime, out var value);
                    value ??= new();
                    value.AddRange(actions);
                }

                if (events.TryAdd(targetTime, actions))
                {
                    _logger.LogDebug("Added {count} events {date}", actions.Count, time);
                }
            }
        }

        public void RemovePrecise(DateTime time, string name)
        {
            var targetTime = GetTargetTime(DateTime.Now);
            if (events.ContainsKey(targetTime))
            {
                events.TryGetValue(targetTime, out var eventList);
                if (eventList is null)
                {
                    _logger.LogError("Precise remove error {time} <{name}>: event list was null", time, name);
                    return;
                }

                var eventToRemove = eventList.FirstOrDefault(e => e.Name == name);
                if (eventToRemove is null)
                {
                    _logger.LogError("Precise remove error {time} <{name}>: didn't find event with provided name", time, name);
                    return;
                }

                eventList.Remove(eventToRemove);
                lock (_lockObj)
                {
                    events[targetTime] = eventList;
                }
            }
        }

        public void RemoveEventsByName(string name)
        {
            var eventsToRemove = events.Where(e => e.Value.Any(e => e.Name == name));

            foreach (var time in eventsToRemove)
            {
                events.Remove(time.Key);
            }
        }

        public void RemoveEventsByTime(DateTime time)
        {
            var targetTime = GetTargetTime(DateTime.Now);
            if (events.ContainsKey(targetTime))
            {
                lock (_lockObj)
                {
                    if (!events.Remove(targetTime))
                        _logger.LogError("Failed to removed events at {time}", targetTime);
                }
            }
        }

        public DateTime GetStartTime() => _startDate;

        private Timer InitTimer()
        {
            var timer = new Timer(async (e) =>
            {
                await ExecuteEvents();
            }, null, new TimeSpan(0), _ticker);

            return timer;
        }

        private Task ExecuteEvents()
        {
            try
            {
                var targetTime = GetTargetTime(DateTime.Now);
                var executionEvents = events.Where(e => e.Key < targetTime);

                if (executionEvents.Any())
                {
                    foreach (var tasks in executionEvents)
                    {
                        foreach (var task in tasks.Value)
                        {
                            // fire and forget
                            _ = Task.Run(task.Event);
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

            return Task.CompletedTask;
        }

        private ulong GetTargetTime(DateTime time)
        {
            return (ulong)(DateTime.Now.Ticks / TICKS_PER_SECOND);
        }
    }
}