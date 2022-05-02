namespace Huppy.Core.Services.EventService
{
    public interface IEventService
    {
        void Initialize();
        void AddEvent(DateTime time, TimedEvent timedEvent);
        void AddEvent(DateTime time, string eventName, Func<Task?> action);
        void AddEvent(DateTime time, string eventName, Task action);
        void AddRange(DateTime time, List<TimedEvent> actions);
        Task RemovePrecise(DateTime time, string name);
        Task RemoveEventsByName(string eventName);
        Task RemoveEventsByTime(DateTime time);
        DateTime GetStartTime();
    }
}