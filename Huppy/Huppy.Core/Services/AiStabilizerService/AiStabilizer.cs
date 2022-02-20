using System.Collections.Concurrent;
using Huppy.Core.Entities;
using Huppy.Core.IRepositories;
using Huppy.Core.Models;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.AiStabilizerService
{
    public class AiStabilizerService : IAiStabilizerService
    {
        private readonly ILogger _logger;
        private readonly IAiUsageRepository _usageRepository;
        private ConcurrentDictionary<ulong, AiUser> _userAiUsage = new();
        private double EstimatedCost = 0;

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
            await AddToCache(UserId, Username);
            EstimatedCost += Response.Length / 4 * 0.00006;
        }

        public async Task<Dictionary<ulong, AiUser>> GetStatistics()
        {
            return _userAiUsage.ToDictionary(p => p.Key, p => p.Value);
        }

        private async Task AddToCache(ulong UserId, string Username)
        {
            if (_userAiUsage.TryGetValue(UserId, out AiUser currentValue))
            {
                _userAiUsage[UserId].Count = currentValue.Count + 1;
            }
            else
            {
                _userAiUsage.TryAdd(UserId, new AiUser { Username = Username, Count = 1 });
            }
        }
    }
}