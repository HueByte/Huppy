using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.PaginatedEmbedService
{
    public class PaginatorEmbedService : IPaginatorEmbedService
    {
        private readonly ILogger<PaginatorEmbedService> _logger;
        public PaginatorEmbedService(ILogger<PaginatorEmbedService> logger)
        {
            _logger = logger;
        }

        public async Task SendPaginatedMessage()
        {

        }
    }
}