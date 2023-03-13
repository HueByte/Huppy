using HuppyService.Service.Protos;

namespace Huppy.Core.Services.HuppyCacheStorage;

public partial class CacheStorageService
{
    public async Task LogUsageAsync(string Username, ulong UserId)
    {
        await AddToCache(UserId, Username);
    }

    public Task<int> GetUserUsage(ulong UserId)
    {
        _userAiUsage.TryGetValue(UserId, out var userUsage);
        return Task.FromResult(userUsage!);
    }

    public Task<int> GetCurrentMessageCount()
    {
        var count = _userAiUsage.Sum(e => e.Value);
        return Task.FromResult(count);
    }

    public Task<Dictionary<ulong, int>> GetAiStatistics()
    {
        return Task.FromResult(_userAiUsage.ToDictionary(p => p.Key, p => p.Value));
    }

    private Task AddToCache(ulong UserId, string Username)
    {
        if (_userAiUsage.TryGetValue(UserId, out var currentValue))
        {
            _userAiUsage[UserId] = currentValue + 1;
        }
        else
        {
            _userAiUsage.TryAdd(UserId, 1);
        }

        return Task.CompletedTask;
    }
}
