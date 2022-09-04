namespace Huppy.Core.Interfaces.IServices
{
    public interface IResourcesService
    {
        Task<string> GetCpuUsage();
        Task<string> GetRamUsage();
        Task<int> GetShardCount();
        Task<TimeSpan> GetUpTime();
        Task<double> GetAverageExecutionTime();
        Task<string> GetBotVersion();
    }
}