using System.Security.Cryptography.X509Certificates;
using Huppy.Core.Entities;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.AiLimiterService
{
    public class AiLimiterService
    {
        private ILogger _logger;
        private List<AiUsage> _usage = new();
        public AiLimiterService(ILogger<AiLimiterService> logger)
        {
            _logger = logger;
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

            // insert to Database;
            // _usage.Add
        }
    }
}