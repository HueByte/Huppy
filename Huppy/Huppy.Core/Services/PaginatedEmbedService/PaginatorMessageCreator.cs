using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.PaginatedEmbedService
{
    public class PaginatorMessageCreator
    {
        private readonly ILogger<PaginatorMessageCreator> _logger;
        public PaginatorMessageCreator(ILogger<PaginatorMessageCreator> logger)
        {
            _logger = logger;
        }


    }
}