using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.AiStabilizerService
{
    // TODO implement white list and limiting for AI
    public class AiStabilizerService : IAiStabilizerService
    {
        private readonly ILogger _logger;

        public AiStabilizerService(ILogger<AiStabilizerService> logger)
        {
            _logger = logger;
        }
    }
}