using Huppy.Core.Entities;
using Huppy.Core.IRepositories;
using Huppy.Core.Models;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.AiLimiterService
{
    public class AiStabilizer : IAiStabilizer
    {
        private ILogger _logger;
        private IAiUsageRepository _usageRepository;
        private List<AiUsage> _usage = new();
        private int Count = 0;
        public AiStabilizer(ILogger<AiStabilizer> logger, IAiUsageRepository usageRepository)
        {
            _logger = logger;
            _usageRepository = usageRepository;
        }

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

            _usage.Add(model);
            await _usageRepository.AddAsync(model);
        }

        public async Task AddToCache(AiUsage model)
        {
            _usage.Add(model);
            Count++;

            if (_usage.Count > 1000)
            {
                _logger.LogWarning("There's over 1000 messages used by AI");
                _usage.RemoveAt(0);
            }
        }
    }
}