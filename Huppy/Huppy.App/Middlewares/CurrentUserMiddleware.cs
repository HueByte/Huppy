using Huppy.Core.Services.CommandService;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Middlewares
{
    public class CurrentUserMiddleware : IMiddleware
    {
        private readonly ILogger _logger;
        public CurrentUserMiddleware(ILogger<CurrentUserMiddleware> logger)
        {
            _logger = logger;
        }
        public async Task AfterAsync(ExtendedShardedInteractionContext context)
        {
        }

        public async Task BeforeAsync(ExtendedShardedInteractionContext context)
        {
        }
    }
}