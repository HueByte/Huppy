using Microsoft.Extensions.DependencyInjection;

namespace Huppy.Core.Services.TimedEventsService
{
    public interface ITimedEventsService
    {
        Task StartTimers();
        void AddJob(Guid jobGuid, object? data, TimeSpan dueTime, TimeSpan period, Func<AsyncServiceScope, object?, Task?> task);
    }
}