using Discord.Interactions;
using Huppy.Kernel;

namespace Huppy.Core.Interfaces;

public interface IMiddleware
{
    Task BeforeAsync(ExtendedShardedInteractionContext context);
    Task AfterAsync(ExtendedShardedInteractionContext context, ICommandInfo commandInfo, IResult result);
}
