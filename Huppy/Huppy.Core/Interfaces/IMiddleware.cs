using Discord.Interactions;
using Huppy.Core.Services.CommandService;

namespace Huppy.Core.Interfaces
{
    public interface IMiddleware
    {
        Task BeforeAsync(ExtendedShardedInteractionContext context);
        Task AfterAsync(ExtendedShardedInteractionContext context, ICommandInfo commandInfo, IResult result);
    }
}