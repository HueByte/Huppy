namespace Huppy.Core.Services.HuppyCache;

public partial class CacheService
{
    public Task AddCacheUser(ulong UserId, string? Username)
    {
        CacheUsers.Add(UserId, Username);
        return Task.CompletedTask;
    }

    public List<string> GetUserNames() => CacheUsers.Values.ToList()!;
}
