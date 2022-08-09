namespace Huppy.Core.Services.EventService
{
    public interface IEventService
    {
        event Func<string[], Task> OnEventsRemoved;
        void Initialize();
        void AddEvent(DateTime time, TimedEvent timedEvent);
        void AddEvent(DateTime time, string eventName, Func<Task?> action);
        void AddEvent(DateTime time, string eventName, Task action);
        void AddRange(DateTime time, List<TimedEvent> actions);
        Task AddEvent(DateTime time, string Name, object? data, Func<object?, Task> job);
        Task RemovePrecise(DateTime? time, string name);
        Task RemoveEventsByName(string eventName);
        Task RemoveEventsByTime(DateTime time);
        DateTime GetStartTime();
    }
}