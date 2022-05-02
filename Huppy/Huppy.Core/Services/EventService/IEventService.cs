namespace Huppy.Core.Services.EventService
{
    public interface IEventService
    {
        void Initialize();
        void AddEvent(DateTime time, TimedEvent timedEvent);
        void AddEvent(DateTime time, string eventName, Func<Task?> action);
        void AddEvent(DateTime time, string eventName, Task action);
        void AddRange(DateTime time, List<TimedEvent> actions);
        void RemoveEventByName(string eventName);
        void RemoveEventByTime(DateTime time);
        DateTime GetStartTime();
    }
}