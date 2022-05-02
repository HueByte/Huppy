namespace Huppy.Core.Services.EventService
{
    public interface IEventService
    {
        void Initialize();
        void AddEvent(DateTime time, Func<Task> action);
        void AddEvent(DateTime time, Task action);
        void AddRange(DateTime time, List<Func<Task>> actions);
        DateTime GetStartTime();
    }
}