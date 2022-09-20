using Huppy.Core.Entities;

namespace Huppy.Core.Interfaces.IServices
{
    public interface ITimedEventsService
    {
        void AddJob(TimedJob job);
        Task RemoveJob(Guid jobId);

    }
}