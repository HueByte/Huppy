using Huppy.Core.Entities;
using Huppy.Core.IRepositories;

namespace Huppy.Core.Services.HuppyCacheService
{
    public partial class CacheService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICommandLogRepository _commandRepository;
        public Dictionary<ulong, string?> UserBasicData;
        public Dictionary<ulong, AiUser> UserAiUsage;

        public CacheService(IUserRepository userRepository, ICommandLogRepository commandRepository)
        {
            _userRepository = userRepository;
            _commandRepository = commandRepository;

            Initialize().GetAwaiter().GetResult();
        }

        public async Task Initialize()
        {
            UserBasicData = new(await _userRepository.GetUsersForCacheAsync());
            UserAiUsage = new(await _commandRepository.GetAiUsage());
        }
    }
}