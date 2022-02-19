using System.Collections.Concurrent;
using Huppy.Core.Entities;
using Huppy.Core.IRepositories;
using Huppy.Core.Models;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.AiStabilizerService
{
    public class AiStabilizerService : IAiStabilizerService
    {
        private ILogger _logger;
        private IAiUsageRepository _usageRepository;
        private ConcurrentDictionary<ulong, int> _userAiUsage = new();
        public AiStabilizerService(ILogger<AiStabilizerService> logger, IAiUsageRepository usageRepository)
        {
            _logger = logger;
            _usageRepository = usageRepository;

            Initialize().GetAwaiter().GetResult();
        }

        private async Task Initialize() =>
            _userAiUsage = new(await _usageRepository.GetUsage());

        public async Task LogUsageAsync(string Prompt, string Username, ulong UserId, string Response)
        {
            AiUsage model = new()
            {
                Prompt = Prompt,
                UserId = UserId,
                Username = Username,
                Response = Response,
                Date = DateTime.UtcNow
            };

            await _usageRepository.AddAsync(model);
            await AddToCache(UserId);
        }

        public async Task<Dictionary<ulong, int>> GetStatistics()
        {
            return _userAiUsage.ToDictionary(p => p.Key, p => p.Value);
        }

        private async Task AddToCache(ulong userId)
        {
            if (_userAiUsage.TryGetValue(userId, out int currentValue))
            {
                _userAiUsage[userId] = currentValue + 1;
            }
            else
            {
                _userAiUsage.TryAdd(userId, 1);
            }
        }
    }
}