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

            CacheUsers = new(await userRepository.GetUsersForCacheAsync());
            UserAiUsage = new(await commandRepository.GetAiUsage());
            PaginatorEntries = new();

            // TODO: configurable in appsettings
            SetMaxMessageCacheSize(MessageCacheSize);
        }
    }
}