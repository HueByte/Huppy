using Huppy.Core.Entities;

namespace Huppy.Core.Services.HuppyCache;

public partial class CacheService
{
    public async Task LogUsageAsync(string Username, ulong UserId)
    {
        await AddToCache(UserId, Username);
    }

    public Task<AiUser> GetUserUsage(ulong UserId)
    {
        _userAiUsage .TryGetValue(UserId, out var user);
        return Task.FromResult(user!);
    }

    public Task<int> GetCurrentMessageCount()
    {
        var count = _userAiUsage .Sum(e => e.Value.Count);
        return Task.FromResult(count);
    }

    public Task<Dictionary<ulong, AiUser>> GetAiStatistics()
    {
        return Task.FromResult(_userAiUsage .ToDictionary(p => p.Key, p => p.Value));
    }

    private Task AddToCache(ulong UserId, string Username)
    {
        if (_userAiUsage .TryGetValue(UserId, out var currentValue))
        {
            _userAiUsage [UserId].Count = currentValue.Count + 1;
        }
        else
        {
            _userAiUsage .TryAdd(UserId, new AiUser { Username = Username, Count = 1 });
        }

        return Task.CompletedTask;
    }
}
