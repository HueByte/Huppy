namespace Huppy.Core.Services.HuppyCacheStorage;

public partial class CacheStorageService
{
    public Task AddCacheUser(ulong UserId, string? Username)
    {
        _cacheUsers.Add(UserId, Username);
        return Task.CompletedTask;
    }

    public List<string> GetUserNames() => _cacheUsers.Values.ToList()!;
}
