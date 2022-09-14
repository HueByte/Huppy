using Discord;
using Discord.Interactions;
using Huppy.Core.Services.HuppyCache;
using Microsoft.Extensions.DependencyInjection;

namespace Huppy.Core.Attributes;
public class RequireDevAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        var cacheService = services.GetRequiredService<CacheService>();

        bool isUserDev = cacheService.DeveloperIds.Contains(context.User.Id);

        return isUserDev
            ? Task.FromResult(PreconditionResult.FromSuccess())
            : Task.FromResult(PreconditionResult.FromError("You are not registered as a developer"));
    }
}
