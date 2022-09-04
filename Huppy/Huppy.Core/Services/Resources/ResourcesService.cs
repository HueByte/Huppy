using System.Diagnostics;
using Discord.WebSocket;
using Huppy.Core.Interfaces.IRepositories;
using Huppy.Core.Interfaces.IServices;
using Microsoft.EntityFrameworkCore;

namespace Huppy.Core.Services.Resources;

public class ResourcesService : IResourcesService
{
    private readonly DiscordShardedClient _client;
    private readonly ICommandLogRepository _commandRepository;
    public ResourcesService(DiscordShardedClient client, ICommandLogRepository commandRepository)
    {
        _client = client;
        _commandRepository = commandRepository;
    }

    public Task<string> GetBotVersion()
    {
        throw new NotImplementedException();
    }

    public async Task<string> GetCpuUsage()
    {
        var startTime = DateTime.UtcNow;
        var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

        await Task.Delay(1000); // gets CPU time between 1000ms

        var endTime = DateTime.UtcNow;
        var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

        var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
        var totalMsPassed = (endTime - startTime).TotalMilliseconds;
        var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
        return string.Concat(Math.Round(cpuUsageTotal * 100, 2), '%');
    }

    public async Task<double> GetAverageExecutionTime()
    {
        // TODO don't include debug commands
        var commandsQuery = await _commandRepository.GetAllAsync();
        var avgTime = await commandsQuery.AverageAsync(e => e.ExecutionTimeMs);

        return Math.Round(avgTime, 2);
    }

    public Task<string> GetRamUsage()
    {
        var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
        long totalBytesOfMemoryUsed = currentProcess.WorkingSet64;

        var result = Math.Round(totalBytesOfMemoryUsed / 1_000_000M, 2);

        return Task.FromResult(string.Concat(result, " MB"));
    }

    public Task<int> GetShardCount()
    {
        var shardCount = _client.Shards.Count;
        return Task.FromResult(shardCount);
    }

    public Task<TimeSpan> GetUpTime()
    {
        Process p = Process.GetCurrentProcess();
        TimeSpan result = new(DateTime.UtcNow.Ticks - p.StartTime.ToUniversalTime().Ticks);

        return Task.FromResult(result);
    }
}
