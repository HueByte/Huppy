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
        UserAiUsage.TryGetValue(UserId, out var user);
        return Task.FromResult(user!);
    }

    public Task<int> GetCurrentMessageCount()
    {
        var count = UserAiUsage.Sum(e => e.Value.Count);
        return Task.FromResult(count);
    }

    public Task<Dictionary<ulong, AiUser>> GetAiStatistics()
    {
        return Task.FromResult(UserAiUsage.ToDictionary(p => p.Key, p => p.Value));
    }

    private Task AddToCache(ulong UserId, string Username)
    {
        if (UserAiUsage.TryGetValue(UserId, out var currentValue))
        {
            UserAiUsage[UserId].Count = currentValue.Count + 1;
        }
        else
        {
            UserAiUsage.TryAdd(UserId, new AiUser { Username = Username, Count = 1 });
        }

        return Task.CompletedTask;
    }
}
