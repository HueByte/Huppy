namespace Huppy.Core.Interfaces.IServices
{
    public interface IResourcesService
    {
        Task<string> GetCpuUsageAsync();
        string GetRamUsage();
        int GetShardCount();
        TimeSpan GetUpTime();
        Task<double> GetAverageExecutionTimeAsync();
        Task<string> GetBotVersionAsync();
    }
}