using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.UrbanService
{
    public class UrbanService : IUrbanService
    {
        private readonly ILogger _logger;
        public UrbanService(ILogger<UrbanService> logger)
        {
            _logger = logger;
        }
    }
}