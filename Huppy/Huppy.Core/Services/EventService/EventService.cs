using System.Collections.Concurrent;

namespace Huppy.Core.Services.EventService
{
    public class EventService : IEventService
    {
        public readonly Dictionary<ulong, List<Func<Task>>> events = new();
        private readonly object _lockObj = new();
        private Timer _timer;
        private DateTime _startDate;
        private readonly TimeSpan _ticker = TimeSpan.FromSeconds(1); // ticks
        private const int TICKS_PER_SECOND = 10000000;

        public EventService()
        {
            _startDate = DateTime.Now;
            _timer = InitTimer();
        }

        public void AddEvent(DateTime time, Func<Task> action)
        {
            AddRange(time, new List<Func<Task>> { action });
        }

        public void AddEvent(DateTime time, Task action)
        {
            AddRange(time, new List<Func<Task>> { async () => await action });
        }

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
            var targetTime = (ulong)(DateTime.Now.Ticks / TICKS_PER_SECOND);
            var keyTasks = events.Where(e => e.Key < targetTime);

            if (keyTasks.Any())
            {
                foreach (var events in keyTasks)
                {
                    var tasks = GenerateTaskSequence(events.Value);

                    await foreach (var @event in tasks)
                    {
                        _ = Task.Run(async () => await @event.Invoke());
                    }
                }
            }

            foreach (var key in keyTasks)
            {
                lock (_lockObj)
                {
                    events.Remove(key.Key);
                }
            }

            Console.WriteLine(targetTime);
        }

        private static async IAsyncEnumerable<Func<Task>> GenerateTaskSequence(List<Func<Task>> tasks)
        {
            foreach (var task in tasks)
            {
                yield return task;
            }
        }
    }
}