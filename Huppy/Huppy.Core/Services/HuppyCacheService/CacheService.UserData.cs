namespace Huppy.Core.Services.HuppyCacheService
{
    public partial class CacheService
    {
        public Task AddBasicUserData(ulong UserId, string? Username)
        {
            UserBasicData.Add(UserId, Username);
            return Task.CompletedTask;
        }

        public List<string> GetUserNames() => UserBasicData.Values.ToList()!;
    }
}