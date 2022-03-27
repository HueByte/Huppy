using System.Collections.Concurrent;
using System.Collections.Specialized;
using Huppy.Core.Entities;
using Huppy.Core.IRepositories;
using Microsoft.Extensions.DependencyInjection;

namespace Huppy.Core.Services.HuppyCacheService
{
    public partial class CacheService
    {
        private readonly IServiceScopeFactory _serviceFactory;
        public Dictionary<ulong, string?> UserBasicData;
        public Dictionary<ulong, AiUser> UserAiUsage;
        public OrderedDictionary PaginatedMessages;

        public CacheService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceFactory = serviceScopeFactory;
        }

        public async Task Initialize()
        {
            using var scope = _serviceFactory.CreateAsyncScope();
            var _commandRepository = scope.ServiceProvider.GetRequiredService<ICommandLogRepository>();
            var _userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var _appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();

            UserBasicData = new(await _userRepository.GetUsersForCacheAsync());
            UserAiUsage = new(await _commandRepository.GetAiUsage());
            PaginatedMessages = new();

            // TODO: configurable in appsettings
            SetMaxMessageCacheSize(1000);
        }
    }
}