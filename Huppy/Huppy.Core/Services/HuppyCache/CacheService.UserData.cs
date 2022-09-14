namespace Huppy.Core.Services.HuppyCache;

public partial class CacheService
{
    public Task AddCacheUser(ulong UserId, string? Username)
    {
        _cacheUsers.Add(UserId, Username);
        return Task.CompletedTask;
    }

    public List<string> GetUserNames() => _cacheUsers.Values.ToList()!;
}
