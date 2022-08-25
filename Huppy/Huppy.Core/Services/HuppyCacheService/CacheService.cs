using System.Collections.Specialized;
using Huppy.Core.Entities;
using Huppy.Core.IRepositories;
using Microsoft.Extensions.DependencyInjection;

namespace Huppy.Core.Services.HuppyCacheService
{
    public partial class CacheService
    {
        private readonly IServiceScopeFactory _serviceFactory;
        public Dictionary<ulong, string?> CacheUsers;
        public Dictionary<ulong, AiUser> UserAiUsage;
        public HashSet<ulong> RegisteredGuildsIds;
        public OrderedDictionary PaginatorEntries;
        private const int MessageCacheSize = 1000;

        public CacheService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceFactory = serviceScopeFactory;
        }

        public async Task Initialize()
        {
            using var scope = _serviceFactory.CreateAsyncScope();
            var commandRepository = scope.ServiceProvider.GetRequiredService<ICommandLogRepository>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var serverRepository = scope.ServiceProvider.GetRequiredService<IServerRepository>();

            CacheUsers = new(await userRepository.GetUsersForCacheAsync());
            UserAiUsage = new(await commandRepository.GetAiUsage());
            RegisteredGuildsIds = new((await serverRepository.GetAllAsync()).Select(guild => guild.Id).ToHashSet());
            PaginatorEntries = new();

            // TODO: configurable in appsettings
            SetMaxMessageCacheSize(MessageCacheSize);
        }
    }
}