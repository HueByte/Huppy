using Discord.Interactions;
using Huppy.Core.Services.CommandService;
using Huppy.Core.Services.ScopedDataService;

namespace Huppy.App.Middlewares
{
    public class ScopedDataMiddleware : IMiddleware
    {
        private readonly IScopedDataService _scopedDataService;
        public ScopedDataMiddleware(IScopedDataService scopedDataService)
        {
            _scopedDataService = scopedDataService;
        }
        public Task AfterAsync(ExtendedShardedInteractionContext context, ICommandInfo commandInfo, IResult result)
        {
            return Task.CompletedTask;
        }

        public Task BeforeAsync(ExtendedShardedInteractionContext context)
        {
            _scopedDataService.User = context.User;
            return Task.CompletedTask;
        }
    }
}