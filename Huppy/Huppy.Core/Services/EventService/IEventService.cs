namespace Huppy.Core.Services.EventService
{
    public interface IEventService
    {
        event Func<string[], Task> OnEventsRemoved;
        void Initialize();
        Task AddEvent(DateTime time, string Name, object? data, Func<object?, Task> job);
        DateTime GetStartTime();
    }
}