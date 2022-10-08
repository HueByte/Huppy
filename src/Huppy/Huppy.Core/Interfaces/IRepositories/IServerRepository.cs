using Discord.Interactions;
using Huppy.Core.Models;
using Huppy.Kernel;

namespace Huppy.Core.Interfaces.IRepositories
{
    [Obsolete]
    public interface IServerRepository : IRepository<ulong, Server>
    {
        Task<Server> GetOrCreateAsync(ShardedInteractionContext DiscordContext);
    }
}