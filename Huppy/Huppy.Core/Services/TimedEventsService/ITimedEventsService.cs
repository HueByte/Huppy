using Microsoft.Extensions.DependencyInjection;

namespace Huppy.Core.Services.TimedEventsService
{
    public interface ITimedEventsService
    {
        Task StartTimers();
        void AddJob(Func<AsyncServiceScope, Task?> task, TimeSpan dueTime, TimeSpan period);
    }
}