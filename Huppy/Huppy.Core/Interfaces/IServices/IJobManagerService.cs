namespace Huppy.Core.Interfaces.IServices
{
    public interface IJobManagerService
    {
        Task StartEventLoop();
        Task StartReminderJobs();
        Task StartActivityControlJobs();
        void MarkInitialized();
    }
}