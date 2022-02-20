using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using Huppy.Core.Entities;
using Huppy.Core.IRepositories;
using Huppy.Core.Models;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.AiStabilizerService
{
    public class AiStabilizerService : IAiStabilizerService
    {
        private readonly ILogger _logger;
        private readonly ICommandLogRepository _commandRepository;
        private ConcurrentDictionary<ulong, AiUser> _userAiUsage = new(); // ai usage cache

        public AiStabilizerService(ILogger<AiStabilizerService> logger, ICommandLogRepository commandLogRepository)
        {
            _logger = logger;
            _commandRepository = commandLogRepository;

            Initialize().GetAwaiter().GetResult();
        }

        // fetch ai usage from database and set as cache
        private async Task Initialize() =>
            _userAiUsage = new(await _commandRepository.GetAiUsage());

        /// <summary>
        /// Adds Ai usage to cache
        /// </summary>
        /// <param name="Username"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task LogUsageAsync(string Username, ulong UserId)
        {
            await AddToCache(UserId, Username);
        }

        /// <summary>
        /// Gets specific user AI usage 
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public Task<AiUser> GetUserUsage(ulong UserId)
        {
            _userAiUsage.TryGetValue(UserId, out var user);
            return Task.FromResult(user!);
        }

        /// <summary>
        /// Gets current Ai message count from cache
        /// </summary>
        /// <returns></returns>
        public Task<int> GetCurrentMessageCount()
        {
            var count = _userAiUsage.Sum(e => e.Value.Count);
            return Task.FromResult(count);
        }

        /// <summary>
        /// Gets current Ai usage cache
        /// </summary>
        /// <returns></returns>
        public Task<Dictionary<ulong, AiUser>> GetAiStatistics() =>
            Task.FromResult(_userAiUsage.ToDictionary(p => p.Key, p => p.Value));

        private Task AddToCache(ulong UserId, string Username)
        {
            if (_userAiUsage.TryGetValue(UserId, out var currentValue))
            {
                _userAiUsage[UserId].Count = currentValue.Count + 1;
            }
            else
            {
                _userAiUsage.TryAdd(UserId, new AiUser { Username = Username, Count = 1 });
            }

            return Task.CompletedTask;
        }
    }
}