using Huppy.Core.Entities;

namespace Huppy.Core.Interfaces.IServices
{
    public interface ITimedEventsService
    {
        //     Task StartTimers();
        //     void AddJob(Guid jobGuid, object? data, TimeSpan dueTime, TimeSpan period, Func<AsyncServiceScope, object?, Task?> task);
        void AddJob(TimedJob job);
        Task RemoveJob(Guid jobId);

    }
}