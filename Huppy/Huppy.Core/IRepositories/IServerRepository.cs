using Discord.Interactions;
using Huppy.Core.Models;

namespace Huppy.Core.IRepositories
{
    public interface IServerRepository : IRepository<ulong, Server>
    {
        Task<Server> GetOrCreateAsync(ShardedInteractionContext DiscordContext);
    }
}