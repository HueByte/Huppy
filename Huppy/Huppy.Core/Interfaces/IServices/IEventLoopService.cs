namespace Huppy.Core.Interfaces.IServices
{
    public interface IEventLoopService
    {
        event Func<string[], Task> OnEventsRemoved;
        TimeSpan Ticker { get; }
        Task Execute();
        Task AddEvent(DateTime time, string Name, object? data, Func<object?, Task> job);
        Task Remove(DateTime time, string eventName);
        Task Remove(ulong time, string eventName);
    }
}