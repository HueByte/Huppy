using Huppy.Core.Entities;

namespace Huppy.Core.Interfaces.IServices
{
    public interface IJobManagerService
    {
        List<TimedJob> GetTimedJobs();
        Task StartEventLoop();
        Task StartReminderJobs();
        Task StartActivityControlJobs();
        void MarkInitialized();
    }
}