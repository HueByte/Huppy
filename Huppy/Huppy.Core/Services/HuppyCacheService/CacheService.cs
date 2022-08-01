using System.Collections.Specialized;
using Huppy.Core.Entities;
using Huppy.Core.IRepositories;
using Microsoft.Extensions.DependencyInjection;

namespace Huppy.Core.Services.HuppyCacheService
{
    public partial class CacheService
    {
        private readonly IServiceScopeFactory _serviceFactory;
        private readonly AppSettings _appSettings;
        public Dictionary<ulong, string?> UserBasicData;
        public Dictionary<ulong, AiUser> UserAiUsage;
        public OrderedDictionary PaginatorEntries;

        public CacheService(IServiceScopeFactory serviceScopeFactory, AppSettings appSettings)
        {
            _serviceFactory = serviceScopeFactory;
            _appSettings = appSettings;
        }

        public async Task Initialize()
        {
            using var scope = _serviceFactory.CreateAsyncScope();
            var _commandRepository = scope.ServiceProvider.GetRequiredService<ICommandLogRepository>();
            var _userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            UserBasicData = new(await _userRepository.GetUsersForCacheAsync());
            UserAiUsage = new(await _commandRepository.GetAiUsage());
            PaginatorEntries = new();

            // TODO: configurable in appsettings
            SetMaxMessageCacheSize(1000);
        }
    }
}