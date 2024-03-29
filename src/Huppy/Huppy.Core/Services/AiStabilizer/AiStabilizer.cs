using Huppy.Core.Interfaces.IServices;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.AiStabilizer
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